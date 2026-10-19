from urllib.request import urlopen, Request
import io, zipfile, struct, json
class Remote(io.RawIOBase):
 def __init__(self,url,size=None,base=0):
  self.url=url;self.pos=0;self.base=base
  if size is None:
   with urlopen(Request(url,method='HEAD',headers={'User-Agent':'Mozilla/5.0'}),timeout=40) as r:size=int(r.headers['Content-Length'])
  self.size=size
 def seekable(self):return True
 def readable(self):return True
 def tell(self):return self.pos
 def seek(self,off,whence=0):
  self.pos=off if whence==0 else self.pos+off if whence==1 else self.size+off
  assert self.pos>=0;return self.pos
 def read(self,n=-1):
  if n<0:n=self.size-self.pos
  if n==0 or self.pos>=self.size:return b''
  end=min(self.pos+n,self.size)-1;start=self.base+self.pos;finish=self.base+end
  with urlopen(Request(self.url,headers={'User-Agent':'Mozilla/5.0','Range':f'bytes={start}-{finish}'}),timeout=90) as r:
   assert r.status==206,(r.status,start,finish)
   b=r.read();assert len(b)==end-self.pos+1;self.pos+=len(b);return b
def inner(kind,model):
 url=f'https://renderpeople.com/sample/free/renderpeople_free_{kind}_people_FBX.zip'
 remote=Remote(url)
 with zipfile.ZipFile(remote) as outer:
  info=outer.getinfo(model);assert info.compress_type==zipfile.ZIP_STORED
  remote.seek(info.header_offset);h=struct.unpack('<4s5H3I2H',remote.read(30));assert h[0]==b'PK\x03\x04'
  base=info.header_offset+30+h[-2]+h[-1]
 return Remote(url,size=info.file_size,base=base)
if __name__=='__main__':
 for kind, model in [('rigged','rp_eric_rigged_001_FBX.zip'),('animated','rp_nathan_animated_003_walking_FBX.zip')]:
  with zipfile.ZipFile(inner(kind,model)) as z:
   print(model,flush=True)
   for f in z.infolist():
    if not f.is_dir():print(f.filename,f.file_size,f.compress_size,flush=True)
