import pathlib,urllib.request,re,json,hashlib,concurrent.futures
base=pathlib.Path(__file__).resolve().parents[1]/'Sources'
pages={'cry':('https://freesound.org/people/SPAudiobooks/sounds/776565/','SPAudiobooks'),'scream':('https://freesound.org/people/Archeos/sounds/325548/','Archeos'),'scrapes':('https://opengameart.org/content/scrapes','AntumDeluge'),'mechanical':('https://opengameart.org/content/mechanical-sounds','Brian MacIntosh (BMacZero)')}
def fetch(item):
 tag,(url,author)=item
 try:
  raw=urllib.request.urlopen(url,timeout=30).read();(base/(tag+'-source.html')).write_bytes(raw);s=raw.decode()
  if 'freesound' in url:
   links=re.findall(r'https://[^\s"\'<>]+-hq\.mp3',s);links=list(dict.fromkeys(links))[:1]
  else:links=list(dict.fromkeys('https://opengameart.org'+v if v.startswith('/') else v for v in re.findall(r'href="([^"<>]+)"',s) if '/sites/default/files/' in v and v.lower().endswith(('.wav','.zip'))))
  out=[]
  for i,link in enumerate(links):
   data=urllib.request.urlopen(link,timeout=30).read();filename=tag+'-'+link.split('/')[-1];(base/filename).write_bytes(data)
   out.append(dict(file=filename,url=link,page=url,author=author,license='CC0 1.0',retrieved='2026-10-08',bytes=len(data),sha256=hashlib.sha256(data).hexdigest(),delivery='Public HQ MP3 preview, lossy' if 'freesound' in url else 'Published downloadable original'))
  return tag,out
 except Exception as e:return tag,{'error':str(e)}
result=dict(concurrent.futures.ThreadPoolExecutor(4).map(fetch,pages.items()));(base/'download-manifest.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
