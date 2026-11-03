"""Import unmodified verse text from eBible.org's public-domain WEB HTML archive."""
import hashlib,html,json,pathlib,re,sys,zipfile
from html.parser import HTMLParser
class Verses(HTMLParser):
 def __init__(self):super().__init__();self.items={};self.current=None;self.skipspan=False;self.stopped=False;self.suppress=0
 def handle_starttag(self,t,a):
  a=dict(a)
  if t=='span' and a.get('class')=='verse':
   self.current=int(a['id'][1:]);self.items[self.current]=[];self.skipspan=True
  elif t=='ul' and self.current is not None:self.stopped=True
  elif t=='a' and a.get('class','').startswith('notemark'):self.suppress+=1
  if t in ('div','br') and self.current is not None:self.items[self.current].append(' ')
 def handle_endtag(self,t):
  if t=='span' and self.skipspan:self.skipspan=False
  if t=='a' and self.suppress:self.suppress-=1
 def handle_data(self,d):
  if self.current is not None and not self.skipspan and not self.stopped and not self.suppress:self.items[self.current].append(d)
root=pathlib.Path(__file__).resolve().parents[1];archive=pathlib.Path(sys.argv[1]);z=zipfile.ZipFile(archive);out=[]
for name in sorted(z.namelist()):
 if not re.fullmatch(r'[A-Z0-9]{3}\d{2,3}\.htm',name):continue
 raw=z.read(name).decode('utf-8-sig');m=re.search(r'<title>World English Bible (.+) (\d+)</title>',raw)
 if not m:continue
 p=Verses();p.feed(raw)
 for number,parts in p.items.items():
  text=' '.join(''.join(parts).split())
  if text:out.append(dict(book=html.unescape(m[1]),chapter=int(m[2]),verse=number,text=text,url='https://ebible.org/engwebp/'+name+'#V'+str(number)))
assert 31000<len(out)<32000,len(out)
folder=root/'Assets/Resources/Guide';folder.mkdir(exist_ok=True)
(folder/'Scripture.json').write_text(json.dumps({'translation':'World English Bible Protestant Edition','verses':out},ensure_ascii=False,separators=(',',':')))
doc=root/'Documentation/Guide/SCRIPTURE-SOURCE.txt';doc.write_text('Public-domain World English Bible Protestant Edition.\nSource: https://ebible.org/Scriptures/engwebp_html.zip\nLicense: https://ebible.org/engwebp/copyright.htm\nArchive SHA256: '+hashlib.sha256(archive.read_bytes()).hexdigest()+'\nUnmodified verse text imported from chapter HTML; navigation and footnotes excluded.\nAI answers are generated paraphrases, not the World English Bible translation.\nVerse records: '+str(len(out))+'\n')
print('Imported',len(out),'verses')
