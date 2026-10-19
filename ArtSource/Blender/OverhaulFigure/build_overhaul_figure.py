"""Original adult Echo surface, metres, no rig or captured channels.
Blender X width, -Y front, Z up. FBX -Z/+Y as other Echo Protocol kits.
"""
import bpy, bmesh, math, os, json
from mathutils import Vector
from mathutils.bvhtree import BVHTree
from math import sin,cos,pi,exp
BASE=os.path.dirname(os.path.abspath(__file__));OUT=os.path.join(BASE,'exports');PRE=os.path.join(BASE,'previews')
os.makedirs(OUT,exist_ok=True);os.makedirs(PRE,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
MAT={'FIG_Jacket':((.11,.15,.14,1),.86,0),'FIG_Stitch':((.060,.078,.070,1),.89,0),'FIG_Trousers':((.076,.078,.072,1),.93,0),'FIG_Skin':((.37,.255,.182,1),.65,0),'FIG_Hair':((.040,.030,.025,1),.94,0),'FIG_Shoe':((.047,.030,.022,1),.67,0),'FIG_Sole':((.020,.019,.016,1),.88,0),'FIG_Eyes':((.027,.025,.023,1),.55,0),'FIG_Metal':((.25,.24,.20,1),.49,.62),'FIG_Sclera':((.45,.42,.35,1),.43,0)}
mats={};pieces=[];details=[]
for name,(color,rough,metal) in MAT.items():
 m=bpy.data.materials.new(name);m.diffuse_color=color;m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=color;p.inputs['Roughness'].default_value=rough;p.inputs['Metallic'].default_value=metal;mats[name]=m
 if name in ['FIG_Jacket','FIG_Trousers']:
  n=m.node_tree.nodes.new('ShaderNodeTexNoise');n.inputs['Scale'].default_value=185;n.inputs['Detail'].default_value=2;b=m.node_tree.nodes.new('ShaderNodeBump');b.inputs['Strength'].default_value=.08;b.inputs['Distance'].default_value=.0015;m.node_tree.links.new(n.outputs['Fac'],b.inputs['Height']);m.node_tree.links.new(b.outputs['Normal'],p.inputs['Normal'])

def mesh(name,verts,faces,mat,detail=False):
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update();o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);me.materials.append(mats[mat])
 for f in me.polygons:f.use_smooth=True
 uv=me.uv_layers.new(name='UVMap');bounds=[(min(v[k] for v in verts),max(v[k] for v in verts)) for k in range(3)];axes=sorted(range(3),key=lambda k:bounds[k][1]-bounds[k][0],reverse=True)[:2]
 for face in me.polygons:
  for li in face.loop_indices:
   co=me.vertices[me.loops[li].vertex_index].co;uv.data[li].uv=tuple((co[k]-bounds[k][0])/max(.001,bounds[k][1]-bounds[k][0]) for k in axes)
 (details if detail else pieces).append(o);return o

def sample(keys,z):
 if z<=keys[0][0]:return keys[0][1:]
 if z>=keys[-1][0]:return keys[-1][1:]
 for k,(a,b) in enumerate(zip(keys,keys[1:])):
  if a[0]<=z<=b[0]:
   t=(z-a[0])/(b[0]-a[0]);length=b[0]-a[0];prev=keys[max(0,k-1)];nxt=keys[min(len(keys)-1,k+2)]
   return tuple((2*t**3-3*t**2+1)*a[i]+(t**3-2*t*t+t)*length*(b[i]-prev[i])/(b[0]-prev[0])+(-2*t**3+3*t*t)*b[i]+(t**3-t*t)*length*(nxt[i]-a[i])/(nxt[0]-a[0]) for i in range(1,len(a)))

def loft(name,keys,mat,n=48,rows=48,deform=None,cap=True,detail=False):
 # keys = z, halfwidth, depth-front, depth-back, x-center, y-center.
 v=[];f=[]
 for j in range(rows+1):
  z=keys[0][0]+(keys[-1][0]-keys[0][0])*j/rows;rx,df,db,cx,cy=sample(keys,z)
  for i in range(n):
   a=i*2*pi/n;x=cx+rx*sin(a);y=cy-(df if cos(a)>=0 else db)*cos(a)
   if deform:x,y,z1=deform(x,y,z,a,cx,cy)
   else:z1=z
   v.append((x,y,z1))
 for j in range(rows):
  for i in range(n):a=j*n+i;b=j*n+(i+1)%n;f.append((a,b,b+n,a+n))
 if cap:f += [tuple(reversed(range(n))),tuple(rows*n+i for i in range(n))]
 return mesh(name,v,f,mat,detail)

def tube(name,centres,radii,mat,segments=16,detail=False):
 v=[];f=[]
 for j,(point,r) in enumerate(zip(centres,radii)):
  p=Vector(point);d=Vector(centres[min(j+1,len(centres)-1)])-Vector(centres[max(j-1,0)]);d.normalize();reference=Vector((0,1,0)) if abs(d.y)<.95 else Vector((1,0,0));u=d.cross(reference).normalized();w=d.cross(u).normalized()
  rx,ry=(r,r) if isinstance(r,(float,int)) else r
  for i in range(segments):a=i*2*pi/segments;q=p+u*cos(a)*rx+w*sin(a)*ry;v.append(tuple(q))
 for j in range(len(centres)-1):
  for i in range(segments):a=j*segments+i;b=j*segments+(i+1)%segments;f.append((a,b,b+segments,a+segments))
 f.extend([tuple(reversed(range(segments))),tuple((len(centres)-1)*segments+i for i in range(segments))]);return mesh(name,v,f,mat,detail)

def seam(name,pts,r=.0016,mat='FIG_Stitch'):
 return tube(name,pts,[r]*len(pts),mat,segments=8,detail=True)

def ellipsoid(name,p,s,mat,detail=False):
 v=[];f=[];n=32;rows=20
 for j in range(rows+1):
  a=-pi/2+j*pi/rows
  for i in range(n):b=i*2*pi/n;v.append((p[0]+s[0]*cos(a)*sin(b),p[1]-s[1]*cos(a)*cos(b),p[2]+s[2]*sin(a)))
 for j in range(rows):
  for i in range(n):a=j*n+i;b=j*n+(i+1)%n;f.append((a,b,b+n,a+n))
 return mesh(name,v,f,mat,detail)

# Adult 1.76 m proportions. A mild habitual slouch and asymmetric weight replace a rigid doll stance.
bodykeys=[(.939,.180,.104,.112,0,.013),(.958,.192,.112,.120,0,.009),(1.020,.194,.123,.131,0,.007),(1.10,.183,.118,.128,-.002,.005),(1.21,.198,.121,.134,-.003,.009),(1.31,.213,.127,.137,-.002,.019),(1.366,.212,.115,.129,-.001,.026),(1.397,.183,.102,.112,0,.030),(1.424,.127,.082,.092,0,.027),(1.451,.076,.059,.067,0,.022),(1.475,.052,.047,.051,0,.016)]
def jacketfold(x,y,z,a,cx,cy):
 front=max(0,cos(a));side=abs(sin(a))
 # Sparse diagonal fold families, strongest at hem/arm and interrupted rather than concentric rings.
 fold=.0023*sin(z*72+a*2.7)*exp(-((z-1.00)/.11)**2)
 fold+=.0036*sin(z*45-abs(x)*34)*exp(-((z-1.24)/.17)**2)*front
 fold+=.0012*sin(z*85+a*6)*exp(-((z-1.39)/.065)**2)*side
 return x+sin(a)*fold,y-cos(a)*fold,z+.0018*sin(a*3+z*9)
loft('Jacket continuous torso',bodykeys,'FIG_Jacket',n=64,rows=72,deform=jacketfold)
# Neck slightly forward of the back; no exposed ball joint.
loft('Neck',[(1.441,.038,.035,.037,0,.01),(1.495,.040,.039,.043,0,.009),(1.546,.044,.041,.047,0,.001)],'FIG_Skin',n=36,rows=20)
# Jacket front zipper follows actual shell and terminates below the chin.
zippts=[]
for j in range(38):
 z=.954+j*(1.453-.954)/37;rx,df,db,cx,cy=sample(bodykeys,z);x,y,zz=jacketfold(cx,cy-df,z,0,cx,cy);zippts.append((x,y-.002,zz))
seam('Zipper tape',zippts,.0024,'FIG_Stitch');seam('Zipper teeth',[(x,y-.0007,z) for x,y,z in zippts],.0009,'FIG_Metal')
ellipsoid('Small zipper slider',(0,-.041,1.44),(.005,.003,.009),'FIG_Metal',True)
# Front pockets have a subtle welt rather than giant cartoon openings.
for side in [-1,1]:
 pts=[]
 for j in range(12):
  t=j/11;x=side*(.065+.075*t);z=1.118-.054*t;rx,df,db,cx,cy=sample(bodykeys,z);a=math.asin(max(-.99,min(.99,(x-cx)/rx)));y=cy-df*cos(a);x,y,zz=jacketfold(x,y,z,a,cx,cy);pts.append((x,y-.0022,zz))
 seam('Slanted pocket welt',pts,.003)
# Hood at rest: soft pouch integrated onto upper back, rim folds around neck.
loft('Resting hood fabric',[(1.323,.036,.008,.020,0,.138),(1.36,.087,.032,.043,0,.134),(1.408,.104,.050,.044,0,.125),(1.45,.086,.041,.035,0,.103),(1.47,.051,.024,.023,0,.089)],'FIG_Jacket',n=40,rows=40)

for side in [-1,1]:
 seam('Drawstring',[(side*.063,-.043,1.435),(side*.069,-.074,1.399),(side*.064,-.097,1.363)],.0015,'FIG_Stitch')
 seam('Drawstring tip',[(side*.064,-.097,1.363),(side*.064,-.098,1.351)],.002,'FIG_Metal')
# Sleeves vary elliptically, continuous with shoulder. Elbows and wrists do not end in sphere joints.
for side in [-1,1]:
 off=.007 if side>0 else 0;armkeys=[(.966+off,.035,.036,.038,side*.250,-.008),(.998+off,.039,.042,.041,side*.250,-.005),(1.055+off,.045,.048,.049,side*.248,.009),(1.15+off,.051,.053,.058,side*.244,.024),(1.22+off,.055,.061,.063,side*.238,.030),(1.305+off,.062,.066,.070,side*.226,.032),(1.365+off,.068,.071,.075,side*.207,.030),(1.397+off,.056,.058,.063,side*.176,.029),(1.422+off,.024,.033,.039,side*.112,.025)]
 def sleevefold(x,y,z,a,cx,cy):
  f=.0018*sin(z*104+a*2.1)*exp(-((z-(1.18+off))/.07)**2)+.0012*sin(z*142-a*3)*exp(-((z-(1.035+off))/.065)**2)
  return x+sin(a)*f,y-cos(a)*f,z
 loft(('Right' if side>0 else 'Left')+' sleeve',armkeys,'FIG_Jacket',n=40,rows=64,deform=sleevefold)
 cuff=[]
 for i in range(33):a=i*2*pi/32;cuff.append((side*.250+.0355*sin(a),-.008-.038*cos(a),.978+off))
 seam('Sleeve cuff seam',cuff,.0023)
 # Shoulder seam follows upper sleeve edge, partly buried after union.
 # Shoulder construction now remains in continuous cloth, no floating drawn seam.
 # Palm faces inward, so its narrow edge is seen in frontal standing view.
 palmkeys=[(.872+off,.019,.029,.026,side*.254,-.015),(.897+off,.024,.034,.031,side*.255,-.012),(.933+off,.025,.033,.030,side*.252,-.010),(.962+off,.020,.025,.025,side*.250,-.008),(.983+off,.017,.022,.023,side*.250,-.007)]
 loft('Relaxed palm',palmkeys,'FIG_Skin',n=28,rows=28)
 # Four fingers are close and slightly curled, with lengths that avoid mitten symmetry.
 for i,(yy,length) in enumerate([(-.035,.057),(-.017,.069),(.000,.066),(.016,.052)]):
  centres=[(side*.252,yy,.911+off),(side*.258,yy-.003,.882+off),(side*.253,yy-.007,.911-length+off)];tube('Grouped relaxed finger',centres,[(.0085,.008),(.008,.0075),(.0055,.006)],'FIG_Skin',12)
 tube('Thumb',[(side*.239,-.029,.946+off),(side*.230,-.047,.921+off),(side*.233,-.052,.894+off)],[(.013,.012),(.010,.010),(.007,.008)],'FIG_Skin',16)

# Trousers: waist hidden by jacket, weight settled predominantly on the right foot.
loft('Trouser hip',[(.931,.148,.093,.104,0,.019),(.958,.159,.111,.121,0,.017),(.997,.165,.115,.128,0,.015)],'FIG_Trousers',n=48,rows=24)
for side in [-1,1]:
 sy=.014 if side>0 else -.013
 keys=[(.090,.047,.051,.052,side*.104,sy),(.136,.061,.058,.065,side*.103,sy),(.22,.066,.064,.069,side*.101,sy+.005),(.37,.067,.064,.070,side*.100,sy+.004),(.49,.068,.069,.080,side*.098,sy-.005),(.56,.072,.075,.080,side*.092,sy-.010),(.69,.078,.084,.093,side*.092,sy+.007),(.81,.090,.096,.111,side*.095,sy+.012),(.92,.096,.110,.118,side*.093,sy+.01),(.98,.091,.105,.118,side*.085,sy+.01)]
 def trouserfold(x,y,z,a,cx,cy):
  f=.0031*sin(z*96+a*2.5)*exp(-((z-.18)/.07)**2)+.0028*sin(z*73-a*3)*exp(-((z-.49)/.07)**2)+.0015*sin(z*24+side*a)
  return x+sin(a)*f,y-cos(a)*f,z+.001*sin(a*3)
 loft('Trouser leg',keys,'FIG_Trousers',n=40,rows=76,deform=trouserfold)
 seam('Trouser outer seam',[(side*(sample(keys,z)[3]*side+sample(keys,z)[0]+.0006),sample(keys,z)[4],z) for z in [.14,.24,.37,.49,.58,.72,.86,.94]],.0012,'FIG_Stitch')
 # Low leather shoes; toes project forward under a trouser cuff, not round feet.
 shoe=[(.018,.055,.151,.084,side*.104,sy),(.032,.057,.155,.086,side*.104,sy),(.065,.057,.145,.081,side*.104,sy),(.091,.050,.097,.070,side*.104,sy),(.124,.043,.050,.054,side*.104,sy),(.155,.036,.038,.042,side*.104,sy)]
 loft('Worn leather shoe',shoe,'FIG_Shoe',n=40,rows=36)
 loft('Rubber sole',[(0,.055,.146,.084,side*.104,sy),(.012,.059,.156,.089,side*.104,sy),(.027,.058,.155,.087,side*.104,sy)],'FIG_Sole',n=40,rows=8)
 for k in range(4):
  yy=sy-.088+k*.015;zz=.095+(k*.009);seam('Simple shoelace',[(side*.104-.029,yy,zz),(side*.104+.029,yy+.005,zz+.001)],.0011,'FIG_Stitch')

# Head: continuous scalp/face surface, with orbital hollows, brow, cheeks, nasal bridge, lips, chin.
headkeys=[(1.520,.027,.037,.045,0,-.004),(1.535,.052,.056,.066,0,-.006),(1.558,.067,.069,.081,0,-.003),(1.588,.076,.081,.093,0,0),(1.615,.076,.084,.098,0,.002),(1.646,.074,.084,.100,0,.003),(1.667,.074,.084,.096,0,.004)]
# Analytic upper cranium avoids the peaked crown produced by tapering a loft to one point.
for j in range(1,33):
 z=1.667+.094*j/32;factor=math.sqrt(max(.000001,1-((z-1.667)/.094)**2));headkeys.append((z,.074*factor,.084*factor,.096*factor,0,.004))
def g(x,z,cx,cz,sx,sz):return exp(-((x-cx)/sx)**2-((z-cz)/sz)**2)
def face_deform(x,y,z,a,cx,cy):
 facing=max(0,cos(a))**7
 delta=0
 delta-=.013*g(x,z,0,1.625,.011,.029) # bridge
 delta-=.016*g(x,z,0,1.602,.015,.011) # tip
 delta-=.009*g(x,z,-.013,1.598,.006,.008)+.009*g(x,z,.013,1.598,.006,.008)
 for xx in [-.032,.032]:
  delta+=.009*g(x,z,xx,1.637,.016,.011) # shallow orbit, not skull hole
  delta-=.0045*g(x,z,xx,1.650,.022,.006) # brow plane
  delta-=.004*g(x,z,xx*1.2,1.604,.025,.018) # cheek
 delta-=.004*g(x,z,0,1.569,.027,.006)+.003*g(x,z,0,1.557,.024,.005)
 delta+=.002*g(x,z,0,1.576,.007,.007) # philtrum
 delta-=.005*g(x,z,0,1.544,.025,.009)
 delta+=.002*g(x,z,-.009,1.596,.0038,.003)+.002*g(x,z,.009,1.596,.0038,.003)
 delta+=.0022*g(x,z,0,1.5645,.026,.0017)
 for side in [-1,1]:delta+=.0014*g(x,z,side*(.020+max(0,1.59-z)*.25),1.581,.0035,.020)
 return x,y+delta*facing,z
loft('Adult head surface',headkeys,'FIG_Skin',n=96,rows=104,deform=face_deform)
# Ears with outer rim and shallow inner concha; no oversized spheres.
for side in [-1,1]:
 ellipsoid('Ear pinna',(side*.076,.003,1.612),(.011,.017,.030),'FIG_Skin')
 seam('Ear helix',[(side*.083,.005,1.640),(side*.087,-.008,1.631),(side*.088,-.011,1.614),(side*.085,-.007,1.592),(side*.080,.001,1.588)],.0032,'FIG_Skin')
 # Inner ear remains a shallow flesh volume, without a dark outlined insert.
# The recorded face is restrained: orbital/brow/cheek planes, no drawn eyes or eyebrows.
# Mouth is a small continuous neutral crease; no smile, frown, or horror teeth.
mouth=[]
for j in range(21):
 t=j/20;x=(t-.5)*.049;z=1.5648+.0002*sin(t*pi);rx,df,db,cx,cy=sample(headkeys,z);a=math.asin(x/rx);y=cy-df*cos(a);x,y,z=face_deform(x,y,z,a,cx,cy);mouth.append((x,y-.0005,z))
# Closed lips are shaped into face; avoid the drawn doll-mouth line.
# Hair cap follows scalp with an irregular but restrained short-hair line.
v=[];f=[];n=96;rows=32
for j in range(rows+1):
 t=j/rows
 for i in range(n):
  a=i*2*pi/n;front=max(0,cos(a));bottom=(1.699-.010*(1-cos(a))/.5 if cos(a)>.5 else (1.632+.058*cos(a)/.5 if cos(a)>=0 else 1.602+.030*(1+cos(a))))+.0015*sin(a*5)+.0008*cos(a*13)
  z=bottom+(1.761-bottom)*t
  rx,df,db,cx,cy=sample(headkeys,min(z,1.759));ripple=.0006*sin(a*43+t*19)*(1-t);offset=.0015+ripple
  x=cx+(rx+offset)*sin(a);y=cy-((df if cos(a)>=0 else db)+offset)*cos(a);v.append((x,y,z+.001*(1-t)))
for j in range(rows):
 for i in range(n):a=j*n+i;b=j*n+(i+1)%n;f.append((a,b,b+n,a+n))
hair=mesh('Short hair scalp',v,f,'FIG_Hair',True)

# Union the large external volumes so transparent Echo shader sees a continuous body,
# not intersecting shoulder/neck/cuff shells. Retain small sewn and facial surface details.
bpy.ops.object.select_all(action='DESELECT')
for o in pieces:o.select_set(True)
bpy.context.view_layer.objects.active=pieces[0];bpy.ops.object.join();body=bpy.context.object;body.name='AdultContinuousSurface'
source_tree=BVHTree.FromPolygons([v.co.copy() for v in body.data.vertices],[tuple(p.vertices) for p in body.data.polygons],all_triangles=False);source_mat=[p.material_index for p in body.data.polygons]
remesh=body.modifiers.new('Continuous outer surface','REMESH');remesh.mode='VOXEL';remesh.voxel_size=.0033;remesh.use_smooth_shade=True;bpy.ops.object.modifier_apply(modifier=remesh.name)
sm=body.modifiers.new('Relax voxel surface','SMOOTH');sm.factor=.45;sm.iterations=3;bpy.ops.object.modifier_apply(modifier=sm.name)
dec=body.modifiers.new('Surface economy','DECIMATE');dec.ratio=.13;dec.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=dec.name)
# Cut material boundaries through the mesh so sleeves/neck/hem do not acquire jagged painted edges.
bm=bmesh.new();bm.from_mesh(body.data)
for cut_z in [.027,.947,1.477]:
 bmesh.ops.bisect_plane(bm,geom=list(bm.verts)+list(bm.edges)+list(bm.faces),dist=.00001,plane_co=(0,0,cut_z),plane_no=(0,0,1),clear_inner=False,clear_outer=False)
bm.to_mesh(body.data);bm.free();body.data.update()
# Voxel remesh drops source assignments: recover material from nearest original surface.
for poly in body.data.polygons:
 center=sum((body.data.vertices[i].co for i in poly.vertices),Vector())/len(poly.vertices);hit=source_tree.find_nearest(center)
 if hit[2] is not None:poly.material_index=source_mat[hit[2]]
 # Keep cloth boundaries tailored after volumetric union, rather than ragged nearest-shell color patches.
 mat_names=[m.name for m in body.data.materials]
 if abs(center.x)<.233 and .88<center.z<1.08:poly.material_index=mat_names.index('FIG_Jacket' if center.z>.947 else 'FIG_Trousers')
 if center.z<.027:poly.material_index=mat_names.index('FIG_Sole')
 if abs(center.x)<.052 and 1.465<center.z<1.52:poly.material_index=mat_names.index('FIG_Skin' if center.z>1.477 else 'FIG_Jacket')
# Recreate spherical/object surface UVs after union; first integration uses named flat PBR slots.
uv=body.data.uv_layers.new(name='UVMap')
for poly in body.data.polygons:
 for li in poly.loop_indices:
  co=body.data.vertices[body.data.loops[li].vertex_index].co;uv.data[li].uv=((math.atan2(co.x,-co.y)+pi)/(2*pi),co.z/1.76)
# Join details into one skinned-looking static renderer, keep material slots for opaque presence.
bpy.ops.object.select_all(action='DESELECT');body.select_set(True)
for o in details:o.select_set(True)
bpy.context.view_layer.objects.active=body;bpy.ops.object.join();figure=bpy.context.object;figure.name='EP_AdultRecordedSurface'
scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
for p in figure.data.polygons:p.use_smooth=True
# Subtle fixed relaxed pose: 1 cm weight shift and a slight head inclination, not animation data.
for vertex in figure.data.vertices:
 co=vertex.co;co.x*=.93+.07*max(0,min(1,(co.z-1.47)/.055));co.x+=.009*max(0,min(1,(co.z-.80)/.85))
 if co.z>1.485:
  blend=min(1,(co.z-1.485)/.045);angle=.032*blend;p=co-Vector((.009,.005,1.50));y=p.y*cos(angle)-p.z*sin(angle);z=p.y*sin(angle)+p.z*cos(angle);p.y=y;p.z=z
  yaw=-.035*blend;x=p.x*cos(yaw)-p.y*sin(yaw);y=p.x*sin(yaw)+p.y*cos(yaw);p.x=x;p.y=y;vertex.co=p+Vector((.009,.005,1.50))
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'EP_AdultRecordedSurface.fbx'),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH'},bake_anim=False)
coords=[v.co for v in figure.data.vertices];bounds=[[min(p[k] for p in coords),max(p[k] for p in coords)] for k in range(3)]
manifest={'date':'2026-10-01','authorship':'Original procedural modelling; no third-party model or real-person likeness','model':'exports/EP_AdultRecordedSurface.fbx','front_source':'-Y','intended_Unity_front':'+Z; preserve importer axis correction','root':'feet midpoint at floor, no armature','bounds_source_xyz':bounds,'height_m':bounds[2][1]-bounds[2][0],'vertices':len(figure.data.vertices),'triangles':sum(len(p.vertices)-2 for p in figure.data.polygons),'materials':{n:{'baseColorLinear':v[0],'roughness':v[1],'metallic':v[2]} for n,v in MAT.items()},'notes':['Static relaxed adult body. No rig or implied recorded articulation.','Large anatomical/clothing surfaces voxel-unioned before export to reduce transparent internal seams.','Small face/cloth features remain surface geometry. Echo may replace every material slot with one shader.','Studio procedural fabric bump is not baked into FBX.']}
json.dump(manifest,open(os.path.join(BASE,'manifest.json'),'w'),indent=2)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'EchoAdultFigure.blend'))
# Neutral studio only. Eye-height cameras expose proportion, collar and side silhouette.
scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True;scene.render.resolution_x=1100;scene.render.resolution_y=1400;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX'
scene.world.color=(.08,.08,.08)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.004));ground=bpy.context.object;gm=bpy.data.materials.new('PreviewGround');gm.diffuse_color=(.08,.085,.08,1);ground.data.materials.append(gm)
for name,p,power,size,color in [('Soft key',(-2,-3,3),250,2,(1,.88,.74)),('Fill',(3,-1,2),110,2,(.68,.80,1)),('Rim',(0,3,3),260,2,(1,.94,.84))]:
 data=bpy.data.lights.new(name,'AREA');data.energy=power;data.size=size;data.color=color;o=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,0,1))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam;cam.data.lens=58
views={'front':((0,-3.9,1.65),(0,0,.95)),'oblique':((-2.5,-3.2,1.65),(0,0,.98)),'side':((-4,0,1.65),(0,0,.98)),'head':((-.25,-1.3,1.65),(0,0,1.56)),'back':((2.0,3.4,1.65),(0,0,.98))}
for name,(pos,target) in views.items():
 cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=os.path.join(PRE,name+'.png');bpy.ops.render.render(write_still=True)
# Uniform cyan solid surface, useful shape test; intentionally not advertised as Unity's Echo shader.
echo=bpy.data.materials.new('PreviewOnly_EchoCyan');echo.use_nodes=True;p=echo.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(.04,.30,.27,1);p.inputs['Roughness'].default_value=.55
for i in range(len(figure.data.materials)):figure.data.materials[i]=echo
cam.location=views['oblique'][0];cam.rotation_euler=(Vector(views['oblique'][1])-cam.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=os.path.join(PRE,'cyan-surface.png');bpy.ops.render.render(write_still=True)
print('FIGURE COMPLETE',json.dumps(manifest),flush=True)
