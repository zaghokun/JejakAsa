import urllib.request,json,sys,base64
h={'Content-Type':'application/json','Accept':'application/json, text/event-stream'}
def call(m,p,i=1):
 d={'jsonrpc':'2.0','method':m,'params':p}
 if i is not None:d['id']=i
 with urllib.request.urlopen(urllib.request.Request('http://127.0.0.1:8080/mcp',json.dumps(d).encode(),h),timeout=90) as r:
  if r.headers.get('Mcp-Session-Id'):h['Mcp-Session-Id']=r.headers['Mcp-Session-Id']
  s=r.read().decode()
  for l in s.splitlines():
   if l.startswith('data: '):
    v=json.loads(l[6:])
    if v.get('id')==i:return v
  return json.loads(s) if s else {}
call('initialize',{'protocolVersion':'2024-11-05','capabilities':{},'clientInfo':{'name':'gui-review','version':'1'}});call('notifications/initialized',{},None)
req=json.loads(base64.b64decode(sys.argv[1]))
if req.get('params',{}).get('name') in ('run_tests','get_test_job'):call('tools/call',{'name':'manage_tools','arguments':{'action':'activate','group':'testing'}})
print(json.dumps(call(req['method'],req['params'])))
