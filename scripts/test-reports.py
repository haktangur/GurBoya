"""F4 reports against isolated synthetic PostgreSQL fixtures only."""
assert project.startswith('gurboya-f1b-')
assert '/Account/Login' in Client().request('/Reports')[2]
assert '/Account/Login' in Client().request('/Reports/Stock')[2]
for query in ['From=bad', 'From=2026-10-11&To=2026-10-10', 'To=9999-12-31']:
    assert client.request('/Reports?' + query)[0] == 400

report_product = create(product_data(**{'Input.Name':'Rapor sınır ürünü','Input.SalePrice':'10','Input.VatIncluded':'true'}))
assert move(report_product, 3)[0] == 200
boundary_ids = [sale_id(complete(sale_input([(report_product,1,{})]))) for _ in range(3)]
assert refund(boundary_ids[1],return_data(details(boundary_ids[1])[1][0],1,0,**{'Input.RefundMethod':'CARD'}))[0] == 200
# Historical fixtures are injected only in this isolated test DB, in one transaction.
sql(f'''BEGIN;
ALTER TABLE app.sales DISABLE TRIGGER USER;
ALTER TABLE app.sales_returns DISABLE TRIGGER USER;
UPDATE app.sales SET "OccurredAt" = CASE "Id"
 WHEN {boundary_ids[0]} THEN '2020-01-01 20:59:59+00'::timestamptz
 WHEN {boundary_ids[1]} THEN '2020-01-01 21:00:00+00'::timestamptz
 WHEN {boundary_ids[2]} THEN '2020-01-02 21:00:00+00'::timestamptz END
WHERE "Id" IN ({','.join(map(str,boundary_ids))});
UPDATE app.sales_returns SET "OccurredAt" = '2020-01-02 21:00:00+00' WHERE "SaleId" = {boundary_ids[1]};
ALTER TABLE app.sales ENABLE TRIGGER USER;
ALTER TABLE app.sales_returns ENABLE TRIGGER USER;
COMMIT;''')

def cells(body, marker):
    row = re.search(marker + r'[^>]*>(.*?)</tr>', body, re.S)[1]
    return [re.sub('<[^>]+>', '', c).strip() for c in re.findall(r'<t[dh][^>]*>(.*?)</t[dh]>', row, re.S)]

code, body, _ = client.request('/Reports?From=2020-01-02&To=2020-01-02')
assert code == 200
assert cells(body, 'id="report-total"') == ['Toplam','10,00 TL','0,00 TL','10,00 TL']
assert cells(body, f'data-product="{report_product}"')[-3:] == ['1','0','1']
code, body, _ = client.request('/Reports?From=2020-01-03&To=2020-01-03')
assert code == 200
assert cells(body, 'data-payment="CASH"')[1:] == ['10,00 TL','0,00 TL','10,00 TL']
assert cells(body, 'data-payment="CARD"')[1:] == ['0,00 TL','10,00 TL','-10,00 TL']
assert cells(body, 'id="report-total"')[1:] == ['10,00 TL','10,00 TL','0,00 TL']
assert client.request('/Reports?From=2019-01-01&To=2019-01-01')[1].count('Bu tarih aralığında satış veya iade yok.') == 1
assert f'data-stock="{report_product}"' in client.request('/Reports/Stock?OnlyEmpty=true')[1]
assert move(report_product,1)[0] == 200
assert f'data-stock="{report_product}"' not in client.request('/Reports/Stock?OnlyEmpty=true')[1]
sql(f'UPDATE app.products SET "IsActive"=false WHERE "Id"={report_product};')
assert f'data-stock="{report_product}"' not in client.request('/Reports/Stock')[1]
assert f'data-stock="{report_product}"' in client.request('/Reports/Stock?ShowInactive=true')[1]
assert f'data-product="{report_product}"' in client.request('/Reports?From=2020-01-02&To=2020-01-03')[1]
print('PASS F4: Authorization, invalid/empty dates, Istanbul midnight boundaries, separate return date/method, stock filters and inactive history')
