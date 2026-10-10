"""F3 scenarios run by test-inventory.py before its password/lockout checks."""

def sale_input(items, **changes):
    data = {'Input.OperationId':str(uuid.uuid4()), 'Input.DiscountKind':'TL', 'Input.DiscountValue':'0', 'Input.PaymentMethod':'CASH'}
    for i, (identifier, quantity, extras) in enumerate(items):
        _, body, _ = client.request(f'/Products/Edit/{identifier}')
        version = Inputs(body).values['Input.Version']
        fields = {'ProductId':str(identifier), 'Version':version, 'Quantity':str(quantity), 'IsTinted':'false',
                  'Color':'Beyaz', 'TintFee':'0', 'DiscountKind':'TL', 'DiscountValue':'0', **extras}
        data.update({f'Input.Lines[{i}].{key}':value for key,value in fields.items()})
    data.update(changes)
    return data

def complete(data, actor=client):
    return actor.post('/Sales/Create?action=complete', data, '/Sales/Create')

def sale_id(result):
    code, body, url = result
    assert code == 200 and '/Sales/Details/' in url, (code,body[-2500:])
    return int(url.rsplit('/',1)[-1])

def details(identifier):
    result = client.request(f'/Sales/Details/{identifier}')
    assert result[0] == 200
    body = result[1]
    return body, [int(x) for x in re.findall(r'data-sale-line="(\d+)"',body)]

def return_data(line_id, quantity, restock=0, **changes):
    return {'Input.OperationId':str(uuid.uuid4()),'Input.Reason':'Sentetik iade','Input.Confirm':'true',
            'Input.RefundMethod':'CASH','Input.Lines[0].SaleLineId':str(line_id),
            'Input.Lines[0].Quantity':str(quantity),'Input.Lines[0].RestockQuantity':str(restock),**changes}

def refund(identifier, data, action='return', actor=client):
    return actor.post(f'/Sales/Details/{identifier}?action={action}',data,f'/Sales/Details/{identifier}')

assert '/Account/Login' in Client().request('/Sales')[2]
assert client.request('/Sales/Create?action=complete',{})[0] == 400
f3_product = create(product_data(**{'Input.Name':'F3 Fırça', 'Input.IsPaint':'false','Input.Unit':'PIECE','Input.PackageLiters':'', 'Input.SalePrice':'10,005','Input.VatIncluded':'true'}))
assert move(f3_product,20)[0] == 200
pending = sale_input([(f3_product,3,{})])
assert client.post('/Sales/Create?action=preview',pending,'/Sales/Create')[0] == 200
assert stock(f3_product)[0] == 20
assert complete({**pending,'Input.PaymentMethod':''})[0] == 400
assert complete({**pending,'Input.Lines[0].Quantity':'0,5'})[0] == 400
identifier = sale_id(complete(pending))
assert 'id="sale-total">30,01' in details(identifier)[0]
assert sale_id(complete(pending)) == identifier
assert stock(f3_product)[0] == 17
assert complete({**pending,'Input.PaymentMethod':'CARD'})[0] == 400
_, lines = details(identifier)
first = return_data(lines[0],1,1)
assert refund(identifier, first)[0] == 200
assert refund(identifier, first)[0] == 200
assert 'id="refund-total">10,01' in details(identifier)[0]
assert stock(f3_product)[0] == 18
assert refund(identifier,return_data(lines[0],1,0))[0] == 200
assert stock(f3_product)[0] == 18
assert refund(identifier,return_data(lines[0],1,1))[0] == 200
assert 'id="refund-total">30,01' in details(identifier)[0]
assert refund(identifier,return_data(lines[0],1,1))[0] == 400
assert stock(f3_product)[0] == 19
print('PASS F3: Preview is stock-neutral, payment gate, fractional rejection, replay, customer rounding and partial/damaged refunds')

# Tint fee entered gross, line and receipt discount, both cash and card.
tint_product = create(product_data(**{'Input.Name':'F3 Boya', 'Input.SalePrice':'100','Input.VatIncluded':'false'}))
assert move(tint_product,10)[0] == 200
cart = sale_input([(tint_product,2,{'IsTinted':'true','TintFee':'23','Color':'RAL 7016','DiscountKind':'PERCENT','DiscountValue':'10'})],
                  **{'Input.DiscountKind':'TL','Input.DiscountValue':'7,70','Input.PaymentMethod':'CARD'})
tinted = sale_id(complete(cart))
body, tint_lines = details(tinted)
assert 'id="sale-total">220,00' in body and 'RAL 7016' in body and 'Kart' in body
assert refund(tinted,return_data(tint_lines[0],1,1))[0] == 400
assert refund(tinted,return_data(tint_lines[0],2,2),'cancel')[0] == 400
assert stock(tint_product)[0] == 8
zero_tinted = sale_id(complete(sale_input([(tint_product,1,{'IsTinted':'true','TintFee':'0'})])))
assert refund(zero_tinted,return_data(details(zero_tinted)[1][0],1,1))[0] == 400
assert complete(sale_input([(f3_product,1,{'IsTinted':'true','TintFee':'1'})]))[0] == 400
assert complete(sale_input([(f3_product,1,{'DiscountKind':'PERCENT','DiscountValue':'101'})]))[0] == 400
assert complete(sale_input([(f3_product,1,{'DiscountValue':'999'})]))[0] == 400
print('PASS F3: Inclusive tint fee, stacked percentage/TL discounts, color snapshot and tinted return/cancel prohibition')

# Same product on multiple rows must be aggregated; no partial writes on failure.
assert complete(sale_input([(f3_product,10,{}),(f3_product,10,{})]))[0] == 400
assert stock(f3_product)[0] == 19
multi = sale_input([(f3_product,1,{}),(tint_product,999,{})])
assert complete(multi)[0] == 400 and stock(f3_product)[0] == 19
stale_sale = sale_input([(f3_product,1,{})])
assert move(f3_product,1)[0] == 200
assert complete(stale_sale)[0] == 400

# Two requests for the last unit: one commit, one stale/stock failure.
last = create(product_data(**{'Input.Name':'F3 Son stok','Input.IsPaint':'false','Input.Unit':'PIECE','Input.PackageLiters':''}))
assert move(last,1)[0] == 200
carts = [sale_input([(last,1,{})]),sale_input([(last,1,{})])]
actors = [clone_client(),clone_client()]
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
    results = list(pool.map(lambda pair: complete(*pair),zip(carts,actors)))
assert sorted(r[0] for r in results) == [200,400]
assert stock(last)[0] == 0
last_sale = sale_id(next(r for r in results if r[0] == 200))
last_line = details(last_sale)[1][0]
requests = [return_data(last_line,1,1),return_data(last_line,1,1)]
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
    results = list(pool.map(lambda pair: refund(last_sale,pair[0],actor=pair[1]),zip(requests,actors)))
assert sorted(r[0] for r in results) == [200,400]
assert stock(last)[0] == 1
print('PASS F3: Aggregate stock, stale carts, multi-line rejection and concurrent last sale/return')

# Tiny amounts, full discount, receipt rounding minimizes loss across rows.
tiny = create(product_data(**{'Input.Name':'F3 Gram','Input.IsPaint':'false','Input.Unit':'GRAM','Input.PackageLiters':'','Input.SalePrice':'0,005','Input.VatIncluded':'true','Input.VatRate':'100'}))
assert move(tiny,100)[0] == 200
tiny_sale = sale_id(complete(sale_input([(tiny,1,{}),(tiny,1,{})])))
assert 'id="sale-total">0,01' in details(tiny_sale)[0]
for line in details(tiny_sale)[1]:
    assert refund(tiny_sale,return_data(line,1,0))[0] == 200
assert 'id="refund-total">0,01' in details(tiny_sale)[0]
free = sale_id(complete(sale_input([(tiny,3,{})],**{'Input.DiscountKind':'PERCENT','Input.DiscountValue':'100'})))
assert 'id="sale-total">0,00' in details(free)[0]
assert refund(free,return_data(details(free)[1][0],3,3))[0] == 200
fraction = sale_id(complete(sale_input([(tiny,3,{})])))
for _ in range(3): assert refund(fraction,return_data(details(fraction)[1][0],1,0))[0] == 200
assert 'id="refund-total">0,01' in details(fraction)[0]

# History survives later product edits; inactive product can be returned without reactivating it.
history_sale = sale_id(complete(sale_input([(f3_product,2,{})],**{'Input.DiscountKind':'PERCENT','Input.DiscountValue':'10'})))
history_body, history_lines = details(history_sale)
old = product_data(**{'Input.Name':'F3 Değişen ad','Input.IsPaint':'false','Input.Unit':'PIECE','Input.PackageLiters':'','Input.SalePrice':'999','Input.VatIncluded':'true','Input.IsActive':'false'})
changed = edit_data(f3_product,old)
assert client.post(f'/Products/Edit/{f3_product}',changed)[0] == 200
assert 'F3 Fırça' in details(history_sale)[0] and 'id="sale-total">18,00' in details(history_sale)[0]
assert refund(history_sale,return_data(history_lines[0],1,1))[0] == 200
cancel_data = return_data(history_lines[0],0,0)
assert refund(history_sale,cancel_data,'cancel')[0] == 200
assert refund(history_sale,cancel_data,'cancel')[0] == 200
assert refund(history_sale,return_data(history_lines[0],0,0),'cancel')[0] == 400
assert 'id="refund-total">18,00' in details(history_sale)[0]
assert 'Ürün pasif' in stock(f3_product)[1]
assert refund(tinted,return_data(history_lines[0],1,0))[0] == 400
print('PASS F3: Minimal receipt rounding, zero/tiny refunds, historical price/name, inactive returns and safe cancellation')
print('PASS F3: Sales HTTP acceptance complete')

# Test-only database fault injection in the isolated Compose project.
import os
import subprocess
project = os.environ['GURBOYA_TEST_PROJECT']
assert project.startswith('gurboya-f1b-') and '.tmp' in Path(env_path).parts
compose_args = ['docker','compose','--env-file',env_path,'-p',project,'-f','compose.yaml','-f','compose.app.yaml']
def sql(command, success=True, error_contains=None):
    result = subprocess.run(compose_args + ['exec','-T','db','psql','-X','-U','gurboya_admin','-d','gurboya','-v','ON_ERROR_STOP=1','-At'],input=command,text=True,capture_output=True)
    assert (result.returncode == 0) == success, result.stderr
    if error_contains: assert error_contains in result.stderr, result.stderr
    return result.stdout.strip()

def ledger_state():
    return sql('''SELECT (SELECT count(*) FROM app.sales), (SELECT count(*) FROM app.sale_lines),
      (SELECT count(*) FROM app.sales_returns), (SELECT count(*) FROM app.return_lines),
      (SELECT count(*) FROM app.operations), (SELECT count(*) FROM app.stock_movements), (SELECT sum("Quantity") FROM app.products);''')

before_failure = ledger_state()
sql('''CREATE FUNCTION app.test_sale_failure() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN
 IF NEW."Kind" IN ('SALE','RETURN') THEN RAISE EXCEPTION 'Synthetic transaction fault'; END IF; RETURN NEW; END $$;
 CREATE TRIGGER test_sale_failure BEFORE INSERT ON app.stock_movements FOR EACH ROW EXECUTE FUNCTION app.test_sale_failure();''')
try:
    failed_sale = sale_input([(tint_product,1,{})])
    assert complete(failed_sale)[0] == 503
    assert ledger_state() == before_failure, 'Sale transaction partially committed'
finally:
    sql('DROP TRIGGER test_sale_failure ON app.stock_movements; DROP FUNCTION app.test_sale_failure();')
# Same operation can succeed after rollback.
recovered_sale = sale_id(complete(failed_sale))
recovered_line = details(recovered_sale)[1][0]
before_failure = ledger_state()
sql('''CREATE FUNCTION app.test_return_failure() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN
 IF NEW."Kind" = 'RETURN' THEN RAISE EXCEPTION 'Synthetic return fault'; END IF; RETURN NEW; END $$;
 CREATE TRIGGER test_return_failure BEFORE INSERT ON app.stock_movements FOR EACH ROW EXECUTE FUNCTION app.test_return_failure();''')
try:
    failed_return = return_data(recovered_line,1,1)
    assert refund(recovered_sale,failed_return)[0] == 503
    assert ledger_state() == before_failure, 'Return transaction partially committed'
finally:
    sql('DROP TRIGGER test_return_failure ON app.stock_movements; DROP FUNCTION app.test_return_failure();')
assert refund(recovered_sale,failed_return)[0] == 200
for table in ['sales','sale_lines','sales_returns','return_lines']:
    sql(f'DELETE FROM app.{table};',False)
    sql(f'UPDATE app.{table} SET "Total" = "Total";',False)
assert sql('''SELECT count(*) FROM app.products p WHERE "Quantity" <> (SELECT coalesce(sum("Delta"),0) FROM app.stock_movements WHERE "ProductId" = p."Id");''') == '0'
# Deferred document guard rejects a header with no lines/movements/operation.
sql('''INSERT INTO app.sales ("OperationId","ActorId","OccurredAt","PaymentMethod","DiscountKind","DiscountValue","Net","Vat","Total")
 SELECT gen_random_uuid(),"Id",now(),'CASH','TL',0,0,0,0 FROM app."AspNetUsers";''',False)
# No further child rows may be appended to completed documents, even zero-value lines.
sql('''INSERT INTO app.sale_lines ("SaleId","Position","ProductId","ProductName","Brand","Color","Unit","Quantity","UnitPrice","VatIncluded","VatRate","IsTinted","TintFee","DiscountKind","DiscountValue","LineDiscount","ReceiptDiscount","Rounding","Net","Vat","Total")
 SELECT "SaleId",50,"ProductId","ProductName","Brand","Color","Unit",1,0,true,0,false,0,'TL',0,0,0,0,0,0,0 FROM app.sale_lines LIMIT 1;''',False, 'Tamamlanan satışa satır eklenemez')
print('PASS F3: Injected sale/return rollback, retry after rollback, immutable DB history, header/append guards and stock reconciliation')
