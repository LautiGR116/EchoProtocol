"""Read-only inventory of serialized scene geometry. No Unity or scene writes.
Not a general Unity parser: prefab-internal meshes are excluded and documented.
"""
import re,json,math
from pathlib import Path
BASE=Path(__file__).resolve().parent
ROOT=BASE.parents[2]
SOURCE=ROOT/'Assets/_Game/Scenes/Production/EchoProtocol_Observation.unity'
text=SOURCE.read_text();blocks={}
for m in re.finditer(r'^--- !u!(\d+) &(\d+)([^\n]*)\n(.*?)(?=^--- !u!|\Z)',text,re.M|re.S):blocks[int(m[2])]=(int(m[1]),m[3],m[4])
def prop(b,k):
 m=re.search(r'^  '+re.escape(k)+r': (.*)$',b,re.M);return m[1] if m else None
def ref(b,k):
 s=prop(b,k);m=re.search(r'fileID: (-?\d+)',s or '');return int(m[1]) if m else 0
def vec(b,k,n=3,default=None):
 s=prop(b,k)
 return [float(v) for v in re.findall(r'[xyzw]: ([^,}]+)',s)] if s else (default or [0]*n)
def mul(a,b):
 x,y,z,w=a;X,Y,Z,W=b
 return [w*X+x*W+y*Z-z*Y,w*Y-x*Z+y*W+z*X,w*Z+x*Y-y*X+z*W,w*W-x*X-y*Y-z*Z]
def rot(q,v):return mul(mul(q,v+[0]),[-q[0],-q[1],-q[2],q[3]])[:3]
gos={i:{'name':prop(b,'m_Name'),'active':prop(b,'m_IsActive')=='1','components':[]} for i,(t,s,b) in blocks.items() if t==1}
ts={i:{'go':ref(b,'m_GameObject'),'parent':ref(b,'m_Father'),'p':vec(b,'m_LocalPosition'),'q':vec(b,'m_LocalRotation',4,[0,0,0,1]),'s':vec(b,'m_LocalScale',3,[1,1,1]),'stripped':'stripped' in s} for i,(t,s,b) in blocks.items() if t==4}
cache={}
def world(i):
 if i in cache:return cache[i]
 a=ts[i];p,q,s=a['p'],a['q'],a['s']
 if a['parent'] in ts:
  P,Q,S=world(a['parent']);p=[x+y for x,y in zip(P,rot(Q,[x*y for x,y in zip(p,S)]))];q=mul(Q,q);s=[x*y for x,y in zip(S,s)]
 cache[i]=(p,q,s);return cache[i]
def path(i):
 a=ts[i];name=gos.get(a['go'],{}).get('name') or ('stripped#'+str(i));return (path(a['parent'])+'/' if a['parent'] in ts else '')+name
def active(i):
 a=ts[i];return gos.get(a['go'],{}).get('active',True) and (active(a['parent']) if a['parent'] in ts else True)
got={a['go']:i for i,a in ts.items() if a['go']}
for i,(t,s,b) in blocks.items():
 gid=ref(b,'m_GameObject')
 if gid in gos and t!=4:gos[gid]['components'].append((i,t,b))
items=[]
for gid,g in gos.items():
 if gid not in got:continue
 tid=got[gid];p,q,s=world(tid);pa=path(tid);cs=[];intersects=False
 for i,t,b in g['components']:
  if t==65:
   sz=vec(b,'m_Size');center=vec(b,'m_Center');wc=[a+b for a,b in zip(p,rot(q,[a*b for a,b in zip(center,s)]))]
   axes=[rot(q,[(sz[k]*s[k]/2 if j==k else 0) for j in range(3)]) for k in range(3)];ext=[sum(abs(a[k]) for a in axes) for k in range(3)]
   bound=[[wc[k]-ext[k],wc[k]+ext[k]] for k in range(3)]
   intersects|=bound[0][1]>=-8.7 and bound[0][0]<=18 and bound[2][1]>=21 and bound[2][0]<=34
   cs.append({'id':i,'type':'BoxCollider','enabled':prop(b,'m_Enabled')=='1','trigger':prop(b,'m_IsTrigger')=='1','size':sz,'center':center,'world_aabb':bound})
  elif t in (23,108,82):cs.append({'id':i,'type':{23:'MeshRenderer',108:'Light',82:'AudioSource'}[t],'enabled':prop(b,'m_Enabled')=='1'})
  elif t==114:cs.append({'id':i,'type':'MonoBehaviour','script':prop(b,'m_Script'),'enabled':prop(b,'m_Enabled')=='1'})
 relevant=(('SE2' in pa or 'Cold ' in pa or 'Parlor' in pa or 'P2 ' in pa or 'SE3' in pa or 'R1 Return' in pa or 'R1 Portal' in pa or 'First Act' in pa) and 19<p[2]<35 and -9<p[0]<20) or intersects
 if relevant:items.append({'id':tid,'gameObject':gid,'name':g['name'],'path':pa,'activeSelf':g['active'],'activeHierarchy':active(tid),'position':p,'rotation':q,'scale':s,'components':cs})
(BASE/'existing-geometry.json').write_text(json.dumps({'source':str(SOURCE.relative_to(ROOT)),'scope':'Read-only serialized transforms and boxes. No physics, prefab-internal meshes, Unity or visual proof. World AABBs are only a candidate overlap aid.','objects':items},indent=2)+'\n')
for a in items:
 if a['path'].count('/')<5 and any(w in a['name'] for w in ('SE2','P2 East','P2 Return','Parlor','Cold','R1 Return','SE3 Return')):
  print(a['name'],[round(v,3) for v in a['position']],[round(v,3) for v in a['scale']],[(c['type'],c.get('enabled'),c.get('trigger')) for c in a['components']])
