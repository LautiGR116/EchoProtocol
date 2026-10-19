"""Final neutral adult Echo representation: static coat silhouette, no invented pose channel."""
import pathlib
exec(pathlib.Path('ArtSource/Blender/Campaign/build_campaign_kit.py').read_text().split('# Blender axes')[0])
def loft(name,rings,mat='Echo',sides=32):
 vertices=[];faces=[]
 for j,(cx,cy,z,rx,ry) in enumerate(rings):
  for i in range(sides):
   a=i*math.pi*2/sides
   fold=1+.018*math.sin(a*9+j*.4)+.012*math.sin(a*15-j*.22)
   vertices.append((cx+rx*math.cos(a)*fold,cy+ry*math.sin(a)*fold,z))
 for j in range(len(rings)-1):
  for i in range(sides):faces.append((j*sides+i,j*sides+(i+1)%sides,(j+1)*sides+(i+1)%sides,(j+1)*sides+i))
 faces.append(tuple(reversed(range(sides))));faces.append(tuple((len(rings)-1)*sides+i for i in range(sides)))
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.materials.append(mats[mat]);mesh.update();obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);parts.append(obj)
 for p in mesh.polygons:p.use_smooth=True
 return obj
loft('Continuous coat',[(0,0,.79,.22,.145),(0,0,.86,.21,.14),(0,0,.99,.195,.135),(0,0,1.13,.183,.123),(0,0,1.27,.215,.13),(0,0,1.37,.225,.11),(0,0,1.42,.18,.09),(0,0,1.46,.074,.066)])
# Long collar and asymmetrical lapels; seams add human scale without facial expression.
for sign in [-1,1]:
 rod('Collar fold',(sign*.05,-.065,1.45),(sign*.105,-.11,1.28),.018,'Echo')
 rod('Lapel',(sign*.105,-.11,1.28),(sign*.018,-.133,1.17),.01,'Echo')
 loft('Trouser leg',[(sign*.105,0,.095,.063,.067),(sign*.107,-.005,.27,.062,.071),(sign*.11,-.018,.44,.073,.078),(sign*.10,0,.60,.081,.085),(sign*.095,0,.85,.095,.095)])
 loft('Sleeve',[(sign*.285,-.027,.89,.052,.055),(sign*.285,-.018,1.02,.057,.063),(sign*.264,-.006,1.15,.067,.073),(sign*.22,0,1.33,.082,.09),(sign*.178,0,1.38,.083,.085)])
 sphere('Hand',(sign*.287,-.025,.838),(.044,.052,.072))
 for f in range(4):rod('Finger',(sign*(.258+f*.017),-.045,.81),(sign*(.26+f*.017),-.033,.757+abs(f-1)*.008),.008,'Echo')
 sphere('Thumb',(sign*.251,-.073,.834),(.015,.021,.035))
 cube('Leather shoe',(sign*.108,-.051,.066),(.143,.285,.105),'Echo',.047)
 # Pockets only affect the representation, not recorded gestures.
 rod('Coat pocket',(sign*.075,-.13,1.025),(sign*.167,-.115,1.05),.007,'Echo')
cyl('Neck',(0,0,1.49),.047,.14,'Echo')
sphere('Skull',(0,.005,1.675),(.102,.102,.138))
sphere('Jaw',(0,-.015,1.595),(.074,.075,.077))
sphere('Nose',(0,-.101,1.665),(.019,.027,.04))
for sign in [-1,1]:sphere('Ear',(sign*.101,0,1.66),(.018,.025,.045))
rod('Coat closing',(0,-.146,.81),(0,-.135,1.20),.005,'Echo')
for z in [1.0,1.09,1.18]:sphere('Button',(0,-.142,z),(.009,.007,.009))
save('RecordedFigure')
for obj in bpy.data.objects:obj.hide_set(False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath('ArtSource/Blender/Campaign/EchoRecordedFigure.blend'))
