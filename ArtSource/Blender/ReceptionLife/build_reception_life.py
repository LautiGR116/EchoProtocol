"""Original reception working-life props, Blender 5.2 LTS, metres.
Source X width/-Y front/Z up. Current Unity importer mirrors X; see README.
No rig, animation, sounds, simulated hair, external media or gameplay changes.
"""
import bpy,os,json,math
from math import sin,cos,pi
from mathutils import Vector
BASE=os.path.dirname(os.path.abspath(__file__));OUT=os.path.join(BASE,'exports');PRE=os.path.join(BASE,'previews')
for p in [OUT,PRE]:os.makedirs(p,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
VALUES={
 'RL_ChairCloth':{'color':(.22,.155,.088,1),'roughness':.90,'metallic':0},
 'RL_ChairWelt':{'color':(.104,.084,.049,1),'roughness':.84,'metallic':0},
 'RL_Armrest':{'color':(.047,.041,.025,1),'roughness':.58,'metallic':0},
 'RL_BlackSteel':{'color':(.035,.044,.035,1),'roughness':.48,'metallic':.6},
 'RL_WornSteel':{'color':(.205,.225,.202,1),'roughness':.45,'metallic':.76},
 'RL_Rubber':{'color':(.011,.016,.012,1),'roughness':.83,'metallic':0},
 'RL_JacketWool':{'color':(.104,.125,.104,1),'roughness':.93,'metallic':0},
 'RL_JacketLining':{'color':(.035,.046,.036,1),'roughness':.61,'metallic':0},
 'RL_JacketEdge':{'color':(.058,.072,.056,1),'roughness':.92,'metallic':0},
 'RL_HornButton':{'color':(.034,.029,.018,1),'roughness':.40,'metallic':0},
 'RL_HangerWood':{'color':(.17,.088,.034,1),'roughness':.48,'metallic':0},
 'RL_ClockEnamel':{'color':(.214,.247,.207,1),'roughness':.44,'metallic':.18},
 'RL_ClockDial':{'color':(.61,.598,.476,1),'roughness':.71,'metallic':0},
 'RL_ClockInk':{'color':(.014,.020,.015,1),'roughness':.60,'metallic':0}}
mats={}
for name,data in VALUES.items():
 m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=data['color'];p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=data['color'];p.inputs['Roughness'].default_value=data['roughness'];p.inputs['Metallic'].default_value=data['metallic'];mats[name]=m
 # Preview micro-weave only; no texture or hidden dependency is required by FBX.
 if name in ['RL_ChairCloth','RL_JacketWool']:
  n=m.node_tree.nodes.new('ShaderNodeTexNoise');n.inputs['Scale'].default_value=250;n.inputs['Detail'].default_value=2
  bump=m.node_tree.nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.12;bump.inputs['Distance'].default_value=.002;m.node_tree.links.new(n.outputs['Fac'],bump.inputs['Height']);m.node_tree.links.new(bump.outputs['Normal'],p.inputs['Normal'])
parts=[];roots=[];manifest={}
def add(o,mat):o.data.materials.append(mats[mat]);parts.append(o);return o
def weighted(o):
 for f in o.data.polygons:f.use_smooth=True
 bpy.context.view_layer.objects.active=o;m=o.modifiers.new('Weighted manufactured normals','WEIGHTED_NORMAL');m.keep_sharp=True;bpy.ops.object.modifier_apply(modifier=m.name)
def cube(n,p,s,mat='RL_BlackSteel',bevel=.004,rot=(0,0,0)):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p,rotation=rot);o=bpy.context.object;o.name=n;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);add(o,mat)
 if bevel:m=o.modifiers.new('Rounded manufactured edge','BEVEL');m.width=bevel;m.segments=3;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=m.name);weighted(o)
 return o
def cyl(n,p,r,h,mat='RL_WornSteel',axis=(0,0,1),vertices=24):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=h,location=p);o=bpy.context.object;o.name=n;o.rotation_euler=Vector(axis).to_track_quat('Z','Y').to_euler();add(o,mat)
 for f in o.data.polygons:f.use_smooth=len(f.vertices)==4
 return o
def sphere(n,p,s,mat):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=12,location=p);o=bpy.context.object;o.name=n;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);add(o,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o
def mesh(n,v,f,mat,smooth=True):
 me=bpy.data.meshes.new(n);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(n,me);bpy.context.collection.objects.link(o);add(o,mat)
 for poly in me.polygons:poly.use_smooth=smooth
 return o
def curve(n,pts,r,mat='RL_BlackSteel',closed=False,res=2):
 cu=bpy.data.curves.new(n,'CURVE');cu.dimensions='3D';cu.resolution_u=2 if len(pts)>16 else 8;cu.bevel_depth=r;cu.bevel_resolution=res;sp=cu.splines.new('BEZIER');sp.bezier_points.add(len(pts)-1)
 for j,(b,p) in enumerate(zip(sp.bezier_points,pts)):
  p=Vector(p);prev=Vector(pts[(j-1)%len(pts)] if closed or j>0 else pts[j]);nxt=Vector(pts[(j+1)%len(pts)] if closed or j<len(pts)-1 else pts[j]);tangent=(nxt-prev)/6
  left=p-tangent;right=p+tangent
  # Geometry curves do not support animation's AUTO_CLAMPED enum; authored
  # handles clamp each segment to its endpoint range, preventing tube overshoot.
  for k in range(3):left[k]=max(min(prev[k],p[k]),min(max(prev[k],p[k]),left[k]));right[k]=max(min(nxt[k],p[k]),min(max(nxt[k],p[k]),right[k]))
  b.co=p;b.handle_left_type='FREE';b.handle_right_type='FREE';b.handle_left=left;b.handle_right=right
 sp.use_cyclic_u=closed;o=bpy.data.objects.new(n,cu);bpy.context.collection.objects.link(o);bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.convert(target='MESH');o=bpy.context.object;add(o,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o
def cloth_thickness(o,thick=.002,lining=None):
 if lining:o.data.materials.append(mats[lining])
 m=o.modifiers.new('Fabric thickness','SOLIDIFY');m.thickness=thick;m.offset=0;m.material_offset=1 if lining else 0;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=m.name)
def power(v,e):return math.copysign(abs(v)**e,v)
def cushion(n,p,s,back=False):
 v=[];f=[];nx=48;ny=24;e=.36
 for j in range(ny+1):
  a=-pi/2+j*pi/ny
  for i in range(nx):
   b=i*2*pi/nx;x=s[0]/2*power(cos(a),e)*power(cos(b),e);y=s[1]/2*power(cos(a),e)*power(sin(b),e);z=s[2]/2*power(sin(a),e)
   if back:
    x*=.98+.08*(z/(s[2]/2));y-=.038*(x/(s[0]/2))**2;y+=.115*z
    y+=.0015*sin(i*.7+j*.9)*abs(cos(a))
   else:
    if z>0:z-=.009*math.exp(-((x/.15)**2+((y+.025)/.15)**2))
    z+=.0014*sin(i*.63+j*1.12)*abs(cos(a));z-=.014*max(0,(-y-.09)/.14)
   v.append((x+p[0],y+p[1],z+p[2]))
 for j in range(ny):
  for i in range(nx):a=j*nx+i;b=j*nx+(i+1)%nx;f.append((a,b,b+nx,a+nx))
 return mesh(n,v,f,'RL_ChairCloth')
def cloth_tube(n,centres,radii,mat='RL_JacketWool'):
 centres=[Vector(p) for p in centres];v=[];f=[];N=28;prior=None
 for j,(p,(rx,ry)) in enumerate(zip(centres,radii)):
  t=(centres[min(j+1,len(centres)-1)]-centres[max(0,j-1)]).normalized()
  u=t.cross(Vector((0,1,0))).normalized() if prior is None else (prior-t*prior.dot(t)).normalized();w=t.cross(u).normalized();prior=u
  for i in range(N):a=i*2*pi/N;wr=.0016*sin(a*5+j*.8)+.0013*sin(a*3-j*1.8);v.append(tuple(p+(rx+wr)*cos(a)*u+(ry+wr)*sin(a)*w))
 for j in range(len(centres)-1):
  for i in range(N):a=j*N+i;b=j*N+(i+1)%N;f.append((a,b,b+N,a+N))
 f.append(tuple(reversed(range(N)))) # shoulder cap lies inside jacket; cuff remains open
 o=mesh(n,v,f,mat);m=o.modifiers.new('Relaxed textile shoulder transition','SUBSURF');m.levels=1;m.render_levels=1;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=m.name);cloth_thickness(o,.002,'RL_JacketLining');return o
def join(name,items):
 bpy.ops.object.select_all(action='DESELECT')
 for o in items:o.select_set(True)
 bpy.context.view_layer.objects.active=items[0];bpy.ops.object.join();o=bpy.context.object;o.name=name;scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.1519,island_margin=.012,scale_to_bounds=True);bpy.ops.object.mode_set(mode='OBJECT');o.data.uv_layers.active.name='UVMap';return o
def export(name,groups,notes,anchors=None):
 global parts
 root=bpy.data.objects.new('EP_RL_'+name,None);bpy.context.collection.objects.link(root);objects=[root]
 for gn,items in groups:o=join(gn,items);o.parent=root;objects.append(o)
 markers={'AXIS_X':(.1,0,0),'AXIS_Y':(0,.1,0),'AXIS_Z':(0,0,.1),'FRONT_BASE':(0,0,0),'FRONT_TIP':(0,-.1,0)};markers.update(anchors or {})
 points={}
 for an,p in markers.items():
  full=name+'__'+an;o=bpy.data.objects.new(full,None);bpy.context.collection.objects.link(o);o.parent=root;o.location=p;o.empty_display_type='PLAIN_AXES';o.empty_display_size=.02;objects.append(o);points[full]=list(p)
 collection=bpy.data.collections.new('Asset / '+name);scene.collection.children.link(collection)
 for o in objects:
  for c in list(o.users_collection):c.objects.unlink(o)
  collection.objects.link(o)
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=root;file='exports/EP_RL_'+name+'.fbx';bpy.ops.export_scene.fbx(filepath=os.path.join(BASE,file),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False)
 ms=[o for o in objects if o.type=='MESH'];coords=[o.matrix_world@v.co for o in ms for v in o.data.vertices];bb=[[min(p[i] for p in coords),max(p[i] for p in coords)] for i in range(3)]
 manifest[name]={'file':file,'bounds_blender_xyz':bb,'dimensions_width_depth_height':[b-a for a,b in bb],'materials':sorted({m.name for o in ms for m in o.data.materials if m}),'mesh_objects':[{'name':o.name,'pivot_blender_xyz':list(o.location),'vertices':len(o.data.vertices),'triangles':sum(len(f.vertices)-2 for f in o.data.polygons),'uv_layers':[u.name for u in o.data.uv_layers]} for o in ms],'anchors_blender_xyz':points,'notes':notes}
 for o in objects:o.hide_render=True;o.hide_set(True)
 roots.append(root);parts=[];print('EXPORTED',name,manifest[name]['dimensions_width_depth_height'],flush=True)

# Cloth clerical chair: low back, formed steel arms, small castors, no gaming silhouette.
cushion('Shallow used seat cushion',(0,-.015,.448),(.495,.458,.090))
cube('Pressed seat pan',(0,-.003,.399),(.450,.395,.021),'RL_BlackSteel',.015)
outline=[]
for i in range(48):a=i*2*pi/48;outline.append((.248*power(cos(a),.36),-.015+.229*power(sin(a),.36),.444-.008*max(0,-sin(a))))
curve('Seat welt',outline,.0022,'RL_ChairWelt',True,1)
cushion('Curved low clerical back',(0,.188,.754),(.440,.073,.370),True)
# Back welt follows a concave front and slight recline, not a floating rectangular wire.
outline=[]
for i in range(48):
 a=i*2*pi/48;xb=.214*power(cos(a),.36);z=.180*power(sin(a),.36);x=xb*(.98+.08*z/.185)
 front_depth=.0365*max(.001,1-(abs(xb)/.220)**(2/.36)-(abs(z)/.185)**(2/.36))**(.36/2)
 y=.188-front_depth-.038*(x/.220)**2+.115*z-.0015;outline.append((x,y,.754+z))
curve('Back upholstery welt',outline,.0020,'RL_ChairWelt',True,1)
for x in [-.115,.115]:curve('Back support steel',[(x,.10,.369),(x,.22,.46),(x,.240,.71),(x,.24,.881)],.013,'RL_BlackSteel')
cube('Back mounting bridge',(0,.239,.746),(.282,.016,.071),'RL_WornSteel',.004)
for x in [-.107,.107]:cyl('Back fastener',(x,.25,.746),.009,.006,'RL_WornSteel',(0,1,0),12)
for x in [-.107,.107]:cyl('Back cushion mounting spacer',(x,.220,.746),.016,.026,'RL_BlackSteel',(0,1,0),16)
for side in [-1,1]:
 x=side*.299;curve('Tubular arm support',[(side*.203,.13,.383),(x,.13,.383),(x,.179,.57),(x,.10,.626),(x,-.16,.626),(x,-.201,.57),(x,-.18,.383),(side*.203,-.18,.383)],.013,'RL_WornSteel')
 for y in [.13,-.18]:cube('Arm to seat mounting plate',(side*.221,y,.386),(.069,.045,.023),'RL_BlackSteel',.003)
 cube('Worn molded arm pad',(x,-.023,.656),(.067,.323,.035),'RL_Armrest',.014)
 for y in [-.105,.095]:cyl('Arm fixing stud',(x,y,.634),.007,.008,'RL_BlackSteel')
cube('Seat tilt mechanism',(0,0,.360),(.155,.198,.062),'RL_BlackSteel',.008)
curve('Height lever',[(.065,-.039,.349),(.185,-.044,.33),(.255,-.075,.315)],.005,'RL_WornSteel',False,1)
cube('Height lever grip',(.254,-.071,.316),(.05,.032,.023),'RL_Armrest',.009)
cyl('Telescopic upper post',(0,0,.306),.028,.102,'RL_WornSteel');cyl('Main chair post',(0,0,.223),.044,.13,'RL_BlackSteel');cyl('Base hub',(0,0,.162),.073,.063,'RL_WornSteel')
for i in range(5):
 a=i*2*pi/5+.17;d=Vector((cos(a),sin(a),0));t=Vector((-sin(a),cos(a),0))
 # Tapered cast spoke with an elliptical cross section.
 v=[];f=[];sections=[(.05,.176,.038,.019),(.12,.165,.032,.017),(.20,.134,.026,.016),(.29,.092,.021,.016)]
 for r,z,w,h in sections:
  for k in range(12):b=k*pi/6;p=d*r+t*(w*cos(b))+Vector((0,0,z+h*sin(b)));v.append(tuple(p))
 for j in range(len(sections)-1):
  for k in range(12):a0=j*12+k;b0=j*12+(k+1)%12;f.append((a0,b0,b0+12,a0+12))
 f.extend([tuple(reversed(range(12))),tuple(36+k for k in range(12))]);mesh('Cast five-star spoke',v,f,'RL_WornSteel')
 p=d*.291;cyl('Castor pivot',(p.x,p.y,.069),.014,.043,'RL_BlackSteel')
 ca=(i*.57-.3);axis=Vector((cos(ca),sin(ca),0));wheel_centre=p+Vector((0,0,.032))
 cube('Castor yoke',(p.x,p.y,.062),(.055,.039,.018),'RL_BlackSteel',.006,rot=(0,0,ca))
 for side in [-1,1]:
  q=wheel_centre+axis*(side*.023);cyl('Dual castor tyre',q,.032,.014,'RL_Rubber',axis,28);cyl('Castor hub',q+axis*(side*.008),.012,.003,'RL_BlackSteel',axis,16)
export('ClericalChair',[('Chair',parts.copy())],'Floor centre root. Original adult-scale low-back fabric swivel chair, metal tubular arms, five paired castors. Seat around .49 m, arm pads .674 m. Static; no sitting/rolling interaction or animation. Use as a slightly pulled-back staff chair; leave genuine knee and seat clearance.',{'SEAT_CENTRE':(0,-.02,.49)})

# Free-standing coat rack: modest metal bent hooks, weighted spread legs.
cyl('Coat rack column',(0,0,.955),.022,1.67,'RL_BlackSteel',vertices=32)
cyl('Lower rack sleeve',(0,0,.24),.032,.26,'RL_BlackSteel')
sphere('Top pole cap',(0,0,1.80),(.032,.032,.031),'RL_Armrest')
for i in range(4):
 a=i*pi/2+.35;d=Vector((cos(a),sin(a),0));curve('Rack spread foot',[(0,0,.28),tuple(d*.11+Vector((0,0,.14))),tuple(d*.24+Vector((0,0,.039))),tuple(d*.29+Vector((0,0,.019)))],.014,'RL_BlackSteel')
 q=d*.29;sphere('Foot rubber cap',(q.x,q.y,.015),(.023,.023,.015),'RL_Rubber')
for i in range(4):
 a=i*pi/2;d=Vector((cos(a),sin(a),0));curve('Bent hat and coat hook',[(0,0,1.57),tuple(d*.14+Vector((0,0,1.66))),tuple(d*.24+Vector((0,0,1.76))),tuple(d*.27+Vector((0,0,1.73)))],.009,'RL_BlackSteel')
 q=d*.27;sphere('Hook rounded end',(q.x,q.y,1.73),(.019,.019,.019),'RL_Armrest')
rack=parts.copy();parts=[]
# Hanger actually catches the front hook and supports the shoulder slope.
curve('Hanger wire hook',[(0,-.27,1.587),(0,-.27,1.653),(.022,-.27,1.703),(0,-.27,1.732),(-.022,-.27,1.713)],.0032,'RL_WornSteel',False,2)
curve('Broad wooden hanger',[(-.218,-.27,1.482),(-.115,-.27,1.530),(0,-.27,1.579),(.112,-.27,1.532),(.218,-.27,1.487)],.010,'RL_HangerWood')
curve('Hanger lower rail',[(-.218,-.27,1.482),(0,-.27,1.455),(.218,-.27,1.487)],.004,'RL_HangerWood')
# Open front, empty adult jacket. Surface wraps around back/sides; no human body inside.
profiles=[(1.58,.080,.061),(1.535,.172,.072),(1.486,.226,.088),(1.40,.198,.108),(1.28,.178,.108),(1.16,.169,.101),(1.035,.176,.088),(.91,.188,.078),(.81,.190,.068),(.782,.182,.064)]
v=[];f=[];N=64;cy=-.262
def jacket_point(j,a):
 z,rx,ry=profiles[j];x=rx*sin(a);y=cy+ry*cos(a);y+=.0045*sin(a*7+j*.6)+.002*sin(a*3-j*.9);z+=.007*sin(a*2+.4)*j/(len(profiles)-1);x+=.008*(j/(len(profiles)-1))**2;return(x,y,z)
for j in range(len(profiles)):
 for i in range(N+1):a=-pi+.17+(2*pi-.34)*i/N;v.append(jacket_point(j,a))
for j in range(len(profiles)-1):
 for i in range(N):a=j*(N+1)+i;f.append((a,a+1,a+N+2,a+N+1))
o=mesh('Empty hanging jacket shell',v,f,'RL_JacketWool');cloth_thickness(o,.0025,'RL_JacketLining')
for side in [-1,1]:
 centres=[(side*.145,cy,1.528),(side*.186,cy,1.515),(side*.226,cy,1.458),(side*.255,cy-.012,1.385),(side*.282,cy+.005,1.25),(side*(.306 if side<0 else .30),cy+.025,1.11),(side*(.303 if side<0 else .348),cy+.041,.95),(side*(.295 if side<0 else .355),cy+.038,.900 if side<0 else .878)]
 radii=[(.003,.003),(.036,.048),(.061,.066),(.066,.061),(.058,.051),(.051,.043),(.044,.033),(.042,.026)]
 cloth_tube('Relaxed empty jacket sleeve',centres,radii)
 # Long outer seam and pressed cuff hem follow the asymmetric sleeve.
 seam=[(p[0]+side*r[0]*.82,p[1]-.014,p[2]) for p,r in zip(centres,radii)];curve('Sleeve side seam',seam,.0014,'RL_JacketEdge',False,1)
 p=centres[-1];cuff=[(p[0]+.041*cos(a),p[1]+.026*sin(a),p[2]+.012) for a in [i*2*pi/24 for i in range(24)]];curve('Cuff stitch',cuff,.0013,'RL_JacketEdge',True,1)
 # Soft, visibly folded lapel rather than a shirt collar.
 sx=side;verts=[(sx*.067,cy-.052,1.567),(sx*.135,cy-.085,1.49),(sx*.091,cy-.111,1.43),(sx*.028,cy-.108,1.195),(sx*.047,cy-.109,1.445)]
 o=mesh('Folded blazer lapel',verts,[(0,1,2,4),(2,3,4)],'RL_JacketWool');cloth_thickness(o,.003)
 curve('Lapel fold',[verts[0],verts[4],verts[3]],.0015,'RL_JacketEdge',False,1)
 edge=[jacket_point(j,side*(pi-.17)) for j in range(len(profiles))];curve('Open front sewn edge',[(x,y-.001,z) for x,y,z in edge],.0015,'RL_JacketEdge',False,1)
# Two horn buttons on one panel, no shirt and no flesh in the garment opening.
for j in [5,6]:
 x,y,z=jacket_point(j,pi-.22);cyl('Jacket horn button',(x,y-.0018,z),.0068,.0026,'RL_HornButton',(0,-1,0),16)
curve('Jacket hem',[jacket_point(len(profiles)-1,-pi+.17+(2*pi-.34)*i/48) for i in range(49)],.0016,'RL_JacketEdge',False,1)
coat=parts.copy();parts=rack+coat
export('CoatRackAndJacket',[('Rack',rack),('HangingAdultJacket',coat)],'Floor root. Empty adult blazer hangs from an actual wooden hanger/wire hook on the front rack arm. Rack and garment are separate meshes for reuse or removal. The garment is a static furnishing, no person, recording or new scare behavior. Front -Y includes jacket lapels; intended corrected Unity +Z.',{'HANGER_CONTACT':(0,-.27,1.73)})

# Institutional clock. All dial geometry and stroke numerals are original curves.
# Root at dial centre, not bottom. Front -Y; corrected X is essential to preserve time.
cyl('Clock rear case',(0,.012,0),.185,.048,'RL_ClockEnamel',(0,-1,0),64)
cyl('Clock dark rim',(0,-.015,0),.187,.027,'RL_BlackSteel',(0,-1,0),64)
cyl('Clock cream dial',(0,-.031,0),.170,.008,'RL_ClockDial',(0,-1,0),64)
circle=[(.177*cos(a),-.037,.177*sin(a)) for a in [i*2*pi/64 for i in range(64)]];curve('Rolled clock bezel',circle,.006,'RL_ClockEnamel',True,1)
for i in range(60):
 a=i*2*pi/60;major=i%5==0;length=.013 if major else .0055;r=.154-length/2;x=r*sin(a);z=r*cos(a)
 cube('Hour baton' if major else 'Minute tick',(x,-.037,z),(.0033 if major else .0011,.0015,length),'RL_ClockInk',0,rot=(0,a,0))
# Stroke outlines for four numerals. They avoid external font files entirely.
digits={
 '1':[[(.16,.84),(.5,1),(.5,0)],[(.17,0),(.83,0)]],
 '2':[[(.05,.78),(.22,1),(.68,1),(.90,.78),(.82,.57),(.10,0),(.90,0)]],
 '3':[[(.05,.90),(.28,1),(.70,.95),(.9,.74),(.61,.52),(.34,.51)],[(.57,.52),(.88,.32),(.84,.10),(.53,0),(.10,.10)]],
 '6':[[(.80,.92),(.55,1),(.22,.78),(.10,.38),(.17,.10),(.43,0),(.76,.11),(.88,.34),(.68,.54),(.22,.50)]],
 '9':[[(.15,.12),(.45,0),(.78,.24),(.88,.65),(.81,.91),(.54,1),(.20,.89),(.12,.66),(.32,.46),(.79,.50)]]}
def digit(c,cx,cz,w=.016,h=.026):
 for path in digits[c]:curve('Dial numeral '+c,[(cx+(x-.5)*w,-.038,cz+(y-.5)*h) for x,y in path],.0009,'RL_ClockInk',False,0)
digit('1',-.010,.124,.009,.024);digit('2',.005,.124,.014,.024);digit('3',.122,0);digit('6',0,-.123);digit('9',-.122,0)
# 03:17: minute=102 degrees, hour=98.5 degrees clockwise from twelve.
def hand(n,angle,length,width,y,tail=.014):
 a=math.radians(angle);d=Vector((sin(a),0,cos(a)));t=Vector((cos(a),0,-sin(a)));pts=[-d*tail-t*width*.40,-d*tail+t*width*.40,d*(length*.78)+t*width*.40,d*length,d*(length*.78)-t*width*.40]
 verts=[(p.x,y,p.z) for p in pts];o=mesh(n,verts,[tuple(range(5))],'RL_ClockInk',False);m=o.modifiers.new('Pressed clock hand thickness','SOLIDIFY');m.thickness=.0018;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=m.name);return o
hour=hand('Fixed hour hand 03h17',98.5,.097,.009,-.041);minute=hand('Fixed minute hand 17m',102,.141,.0045,-.044)
cyl('Clock spindle',(0,-.047,0),.009,.007,'RL_ClockInk',(0,-1,0),24)
# No second hand, sound or glass plane; glass would only add sorting/label problems.
export('WallClock0317',[('ClockFixed0317',parts.copy())],'Wall clock diameter .374 m, root at dial centre. Permanently set to 03:17: hour 98.5 deg, minute 102 deg clockwise from twelve. No second hand, ticking sound, glass shader or animation. Original geometric stroke numerals at 12/3/6/9. Correct imported X mirror or the apparent time will reverse.',{'CLOCK_CENTRE':(0,0,0),'HOUR_TIP':tuple(Vector((sin(math.radians(98.5)),0,cos(math.radians(98.5))))*.097+Vector((0,-.041,0))),'MINUTE_TIP':tuple(Vector((sin(math.radians(102)),0,cos(math.radians(102))))*.141+Vector((0,-.044,0)))})

with open(os.path.join(BASE,'manifest.json'),'w') as f:json.dump({'date':'2026-10-01','authorship':'Original project meshes and geometric clock numerals. No downloads, external textures/fonts or paid services. Preview micro-weave is not baked/exported.','axis_contract':'Blender X width, -Y front, Z up. FBX -Z/+Y; current Unity importer maps (-X,Z,-Y). Negate imported root localScale.x once, retain imported rotation/scale magnitudes, then placementRotation * importedRotation. Corrected assembly map (X,Z,-Y), front +UnityZ. Check FRONT_TIP - FRONT_BASE and clock hand/axis anchors.','materials':VALUES,'assets':manifest},f,indent=2)
for o in [roots[0]]+list(roots[0].children_recursive):o.hide_set(False);o.hide_render=False
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'EchoReceptionLife.blend'))
for o in [roots[0]]+list(roots[0].children_recursive):o.hide_set(True);o.hide_render=True

scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True;scene.render.resolution_x=1200;scene.render.resolution_y=1100;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX';scene.world.color=(.09,.09,.09)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.004));ground=bpy.context.object;ground.name='Preview ground';m=bpy.data.materials.new('Preview ground matte');m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.075,.083,.072,1);m.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.9;ground.data.materials.append(m)
for n,p,power,size,col in [('Key',(-3,-4,5),650,4,(1,.85,.67)),('Fill',(4,-2,3),380,3,(.66,.80,1)),('Rim',(1,3,4),550,3,(1,.92,.75))]:
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.size=size;d.color=col;o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,0,.8))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();camera=bpy.context.object;scene.camera=camera
def show(i):
 for j,root in enumerate(roots):
  for o in [root]+list(root.children_recursive):o.hide_set(i!=j);o.hide_render=i!=j
def render(n,p,target,lens=52):
 camera.location=p;camera.rotation_euler=(Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.lens=lens;scene.render.filepath=os.path.join(PRE,n+'.png');bpy.ops.render.render(write_still=True)
show(0);render('ClericalChair',(-1.30,-1.90,1.32),(0,0,.49),54);render('Chair-rear',(1.32,1.77,1.1),(0,0,.5),54)
show(1);render('CoatRackAndJacket',(-1.72,-2.74,1.65),(0,-.12,.94),53);render('Jacket-close',(.75,-1.54,1.61),(0,-.26,1.22),56)
show(2);ground.hide_render=True;render('WallClock0317',(0,-.80,.015),(0,0,0),58)
print('RECEPTION LIFE COMPLETE',OUT,flush=True)
