"""Generate original fixed tour and hesitation recordings with the official course voice service.
Packets are saved temporarily for Unity/Concentus decoding; no speech library is bundled.
"""
import base64,json,pathlib,socketio,time,uuid
root=pathlib.Path(__file__).resolve().parents[1];out=pathlib.Path('/private/tmp/tabernacle-conversation-audio');out.mkdir(exist_ok=True);state={};s=socketio.Client(reconnection=False,request_timeout=30)
@s.on('audio_frame')
def audio(d):
 if d.get('context_id')!=state.get('id'):return
 state['last']=time.monotonic();state['packets'].append({'frame':base64.b64encode(d['frame']).decode(),'number':d['frame_number'],'total':d['total_frames']})
@s.on('ai_full_response_done')
def done(d):
 if d.get('context_id')==state.get('id'):state['done']=True;state['text']=d.get('final_text','')
@s.on('error_response')
def error(d):state['error']=d
items=json.loads((root/'Documentation/Guide/narration.json').read_text())
items=[dict(name='Guide/'+i['name'],text=i['text']) for i in items if i['name']!='welcome']
items += [dict(name='Reactions/'+name,text=text) for name,text in [('hmm','Hmm.'),('umm','Umm.'),('well','Well...'),('let-me-think','Let me think.'),('give-me-a-second','Give me a second to think...'),('connection',"I couldn't quite connect just then. Please try again.")]]
s.connect('https://chat.vn.ugavel.com',transports=['websocket'],wait_timeout=30)
try:
 for item in items:
  path=out/(item['name'].replace('/','-')+'.json')
  if path.exists():continue
  state.clear();state.update(id=uuid.uuid4().hex,packets=[],last=time.monotonic());s.emit('chat_message',{'context_id':state['id'],'agent_system_prompt':'You are a speech reader. Repeat the supplied text exactly. No introduction, no additions, no commentary.','prompt':item['text'],'voice_mode':'bm_daniel','force_response_type':'text'});end=time.monotonic()+90
  while time.monotonic()<end:
   s.sleep(.05)
   if state.get('error'):raise RuntimeError(str(state['error']))
   if state.get('done') and state['packets'] and time.monotonic()-state['last']>1.5 and state['packets'][-1]['number']==state['packets'][-1]['total']-1:break
  if not state.get('done') or not state['packets']:raise RuntimeError('No complete audio for '+item['name'])
  if state['text'].split()!=item['text'].split():raise RuntimeError('Voice reader changed '+item['name']+': '+state['text'])
  path.write_text(json.dumps(dict(name=item['name'],text=state['text'],packets=state['packets'])));print(item['name'],len(state['packets']),'frames',flush=True)
finally:s.disconnect()
