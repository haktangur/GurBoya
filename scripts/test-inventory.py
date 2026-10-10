"""Authenticated HTTP acceptance tests. Synthetic fixtures only; no external packages."""
import concurrent.futures
import copy
import html
from html.parser import HTMLParser
import http.cookiejar
from pathlib import Path
import re
import sys
import urllib.error
import urllib.parse
import urllib.request
import uuid
from decimal import Decimal

base_url, env_path = sys.argv[1:]
env = dict(line.split('=', 1) for line in Path(env_path).read_text().splitlines() if '=' in line)

class Inputs(HTMLParser):
    def __init__(self, body):
        super().__init__()
        self.values = {}
        self.feed(body)
    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == 'input' and 'name' in attrs:
            self.values.setdefault(attrs['name'], attrs.get('value', ''))

class Client:
    def __init__(self, jar=None):
        self.jar = jar or http.cookiejar.CookieJar()
        self.opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(self.jar))
    def request(self, path, data=None):
        request = urllib.request.Request(base_url + path, data=urllib.parse.urlencode(data).encode() if data is not None else None)
        try:
            response = self.opener.open(request, timeout=15)
        except urllib.error.HTTPError as error:
            response = error
        return response.code, html.unescape(response.read().decode()), response.url
    def post(self, path, data, token_path=None):
        _, page, _ = self.request(token_path or path)
        token = Inputs(page).values['__RequestVerificationToken']
        return self.request(path, {**data, '__RequestVerificationToken': token})

client = Client()
code, body, url = client.request('/Products')
assert '/Account/Login' in url and code == 200
code, _, _ = client.request('/Account/Login', {'Password': env['ADMIN_PASSWORD']})
assert code == 400, 'CSRF guard missing'
code, body, _ = client.post('/Account/Login', {'Password': 'wrong-password'})
assert code == 400 and 'Şifre hatalı' in body
code, body, url = client.post('/Account/Login', {'Password': env['ADMIN_PASSWORD']})
assert code == 200 and '/Products' in url
print('PASS: Authentication, wrong password and CSRF rejection')

def product_data(**changes):
    data = {'Input.RequestId': str(uuid.uuid4()), 'Input.Version': '0', 'Input.Name': 'Deneme Boya',
            'Input.Brand': 'Elle Marka', 'Input.Color': 'Beyaz', 'Input.Category': 'Boya',
            'Input.IsPaint': 'true', 'Input.PackageLiters': '2,5', 'Input.Unit': 'BOX',
            'Input.PurchasePrice': '80', 'Input.SalePrice': '100', 'Input.VatRate': '15',
            'Input.VatIncluded': 'false', 'Input.IsActive': 'true', 'Input.Reason': 'Test başlangıç'}
    data.update(changes)
    return data

def create(data):
    code, body, url = client.post('/Products/Edit', data)
    assert code == 200 and '/Products/Details/' in url, (code, body[-1500:])
    return int(url.rsplit('/', 1)[-1])

def stock(identifier):
    code, body, _ = client.request(f'/Products/Details/{identifier}')
    assert code == 200
    return Decimal(re.search(r'id="stock-quantity">([^<]+)', body)[1].replace(',', '.')), body

def move(identifier, delta, kind='RECEIPT', operation=None, actor=client):
    return actor.post(f'/Products/Details/{identifier}', {'OperationId': operation or str(uuid.uuid4()),
                     'Quantity': str(delta), 'Kind': kind, 'Reason': 'Sentetik stok testi'})

def edit_data(identifier, data, **changes):
    _, body, _ = client.request(f'/Products/Edit/{identifier}')
    values = Inputs(body).values
    return {**data, 'Input.RequestId': values['Input.RequestId'], 'Input.Version': values['Input.Version'], **changes}

paint_data = product_data()
paint = create(paint_data)
assert create(paint_data) == paint, 'Duplicate product created'
changed = {**paint_data, 'Input.Name': 'Farklı içerik'}
assert client.post('/Products/Edit', changed)[0] == 400
quantity, body = stock(paint)
assert quantity == 0 and 'Stokta yok' in body and '115,00 TL' in body
preview = client.post('/Products/Edit?handler=Preview', product_data(**{'Input.VatIncluded': 'true', 'Input.SalePrice': '115'}), '/Products/Edit')
assert preview[0] == 200 and '100,00 TL' in preview[1] and '15,00 TL' in preview[1] and '115,00 TL' in preview[1]
assert client.post('/Products/Edit', product_data(**{'Input.VatRate':'101'}))[0] == 400
assert client.post('/Products/Edit', product_data(**{'Input.SalePrice':'NaN'}))[0] == 400
assert client.post('/Products/Edit', product_data(**{'Input.PackageLiters':'', 'Input.Name':'Eksik litre'}))[0] == 400
assert client.post('/Products/Edit', product_data(**{'Input.SalePrice':'1.234,56'}))[0] == 400
print('PASS: Manual paint fields, Turkish decimals, VAT modes and invalid values')

nails = create(product_data(**{'Input.Name':'Kutu çivi', 'Input.IsPaint':'false', 'Input.PackageLiters':'', 'Input.Color':'', 'Input.Unit':'BOX'}))
grams = create(product_data(**{'Input.Name':'Gram çivi', 'Input.IsPaint':'false', 'Input.PackageLiters':'', 'Input.Color':'', 'Input.Unit':'GRAM', 'Input.SalePrice':'0,025'}))
pieces = create(product_data(**{'Input.Name':'Fırça', 'Input.IsPaint':'false', 'Input.PackageLiters':'', 'Input.Color':'', 'Input.Unit':'PIECE'}))
for identifier in [paint, nails, grams, pieces]:
    assert move(identifier, '0,5')[0] == 400
    assert move(identifier, '0.5')[0] == 400
assert move(grams, 1)[0] == 200
operation = str(uuid.uuid4())
assert move(paint, 12, operation=operation)[0] == 200
assert move(paint, 12, operation=operation)[0] == 200
assert stock(paint)[0] == 12
assert move(paint, 13, operation=operation)[0] == 400
assert move(paint, -13, 'ADJUSTMENT')[0] == 400
assert stock(paint)[0] == 12
assert move(paint, -12, 'ADJUSTMENT')[0] == 200
assert 'Stokta yok' in stock(paint)[1]
assert move(paint, 1)[0] == 200
print('PASS: Box/piece/gram integer units, replay protection, stockout and replenishment')

# Two independent sessions attempt to consume the same last unit.
def clone_client():
    jar = http.cookiejar.CookieJar()
    for cookie in client.jar:
        jar.set_cookie(copy.copy(cookie))
    return Client(jar)
clients = [clone_client(), clone_client()]
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
    results = list(pool.map(lambda actor: move(paint, -1, 'ADJUSTMENT', actor=actor), clients))
assert sorted(result[0] for result in results) == [200, 400]
assert stock(paint)[0] == 0
print('PASS: Concurrent last-unit adjustments: exactly one succeeds')

stale = edit_data(paint, paint_data, **{'Input.SalePrice':'200'})
assert move(paint, 5)[0] == 200
assert client.post(f'/Products/Edit/{paint}', stale)[0] == 400
invalid_unit = edit_data(paint, paint_data, **{'Input.IsPaint':'false','Input.Unit':'GRAM'})
assert client.post(f'/Products/Edit/{paint}', invalid_unit)[0] == 400
new_price = edit_data(paint, paint_data, **{'Input.SalePrice':'115','Input.VatIncluded':'true','Input.Reason':'KDV dahil düzeltme'})
assert client.post(f'/Products/Edit/{paint}', new_price)[0] == 200
assert client.post(f'/Products/Edit/{paint}', new_price)[0] == 200
_, body = stock(paint)
assert '100,00 TL' in body and '15,00 TL' in body and 'KDV dahil düzeltme' in body and 'Test başlangıç' in body
inactive = edit_data(paint, new_price, **{'Input.IsActive':'false','Input.Reason':'Pasifleştirme'})
assert client.post(f'/Products/Edit/{paint}', inactive)[0] == 200
assert move(paint, 1)[0] == 400
assert 'Ürün pasif' in stock(paint)[1]
active = edit_data(paint, new_price, **{'Input.IsActive':'true','Input.Reason':'Yeniden etkinleştirme'})
assert client.post(f'/Products/Edit/{paint}', active)[0] == 200
assert move(paint, 1)[0] == 200
assert stock(paint)[0] == 6
assert 'Kutu çivi' in client.request('/Products?Search='+urllib.parse.quote('Kutu çivi'))[1]
print('PASS: Concurrency version, immutable unit, price history, inactive/reactivated product and search')

# Count sets an absolute total and replays without reapplying after later receipts.
count_op = str(uuid.uuid4())
assert move(nails, 3, 'COUNT', operation=count_op)[0] == 200
assert move(nails, 1)[0] == 200
assert move(nails, 3, 'COUNT', operation=count_op)[0] == 200
assert stock(nails)[0] == 4
assert move(nails, 0, 'COUNT')[0] == 200
assert stock(nails)[0] == 0
assert move(nails, 0, 'COUNT')[0] == 200
assert move(nails, -1, 'COUNT')[0] == 400
print('PASS: Absolute stock count, zero count and safe replay')

# Changing the tax rate stores a new price snapshot without rewriting older rates.
rate_change = edit_data(paint, new_price, **{'Input.VatRate':'20', 'Input.Reason':'Yeni KDV oranı'})
assert client.post(f'/Products/Edit/{paint}', rate_change)[0] == 200
assert '95,8333 TL' in stock(paint)[1]
assert 'KDV dahil düzeltme' in stock(paint)[1]

import runpy
runpy.run_path(str(Path(__file__).with_name('test-sales.py')), init_globals=globals())

new_password = 'Next9' + env['ADMIN_PASSWORD']
assert client.post('/Account/Password', {'CurrentPassword':env['ADMIN_PASSWORD'], 'NewPassword':new_password, 'ConfirmPassword':'different'})[0] == 400
assert client.post('/Account/Password', {'CurrentPassword':env['ADMIN_PASSWORD'], 'NewPassword':new_password, 'ConfirmPassword':new_password})[0] == 200
assert client.post('/Account/Logout', {})[0] == 200
assert '/Account/Login' in client.request('/Products')[2]
assert client.post('/Account/Login', {'Password':env['ADMIN_PASSWORD']})[0] == 400
assert client.post('/Account/Login', {'Password':new_password})[0] == 200
print('PASS: Rate correction, password change and logout/login')

# Store a synthetic authenticated cookie only in ignored private test folder for recreation test.
jar = http.cookiejar.MozillaCookieJar(str(Path(env_path).parent/'session.cookies'))
for cookie in client.jar: jar.set_cookie(cookie)
jar.save(ignore_discard=True, ignore_expires=True)
# curl's Netscape format requires a numeric expiry; 0 means a session cookie.
cookie_path = Path(env_path).parent/'session.cookies'
lines = cookie_path.read_text().splitlines()
for index, line in enumerate(lines):
    if line and not line.startswith('#'):
        columns = line.split('\t')
        if columns[4] == '': columns[4] = '0'
        lines[index] = '\t'.join(columns)
cookie_path.write_text('\n'.join(lines)+'\n')
locked_client = Client()
for _ in range(5):
    assert locked_client.post('/Account/Login', {'Password':'incorrect-again'})[0] == 400
assert locked_client.post('/Account/Login', {'Password':new_password})[0] == 400
print('PASS: Five failed logins temporarily lock the account')
print('PASS: HTTP inventory acceptance complete')
