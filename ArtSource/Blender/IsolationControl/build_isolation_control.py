"""Original manual isolation machinery: metric, X width / -Y front / Z up.

No outside media, add-ons, fonts, or services. Original project mesh helpers
are copied from RecoveryOffice so this source rebuilds independently.
"""
import bpy, os, json, math
from math import sin, cos, pi
from mathutils import Vector, Matrix
BASE=os.path.dirname(os.path.abspath(__file__));OUT=os.path.join(BASE,'exports');PRE=os.path.join(BASE,'previews')
for p in [OUT,PRE]:os.makedirs(p,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
VALUES={
 'IC_Enamel':{'color':(.135,.170,.148,1),'roughness':.57,'metallic':.30},
 'IC_DoorEnamel':{'color':(.255,.273,.218,1),'roughness':.60,'metallic':.18},
 'IC_EdgeMetal':{'color':(.255,.268,.233,1),'roughness':.43,'metallic':.77},
 'IC_DarkMetal':{'color':(.061,.074,.064,1),'roughness':.55,'metallic':.58},
 'IC_Bakelite':{'color':(.030,.019,.011,1),'roughness':.40,'metallic':0},
 'IC_Rubber':{'color':(.011,.013,.011,1),'roughness':.84,'metallic':0},
 'IC_DialIvory':{'color':(.67,.653,.542,1),'roughness':.79,'metallic':0},
 'IC_DialInk':{'color':(.010,.013,.010,1),'roughness':.70,'metallic':0},
 'IC_Needle':{'color':(.205,.052,.021,1),'roughness':.47,'metallic':.20},
 'IC_Brass':{'color':(.26,.181,.081,1),'roughness':.49,'metallic':.70}}
mats={};parts=[];groups=[]
for n,d in VALUES.items():
 m=bpy.data.materials.new(n);m.use_nodes=True;m.diffuse_color=d['color'];p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=d['color'];p.inputs['Roughness'].default_value=d['roughness'];p.inputs['Metallic'].default_value=d['metallic'];mats[n]=m

def add(o,mat):o.data.materials.append(mats[mat]);parts.append(o);return o
def mesh(n,v,f,mat,smooth=True):
 me=bpy.data.meshes.new(n);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(n,me);bpy.context.collection.objects.link(o);add(o,mat)
 for p in me.polygons:p.use_smooth=smooth
 return o
def lathe(n,profile,mat,N=40):
 v=[(r*cos(i*2*pi/N),r*sin(i*2*pi/N),z) for r,z in profile for i in range(N)];f=[]
 for j in range(len(profile)-1):
  for i in range(N):a=j*N+i;b=j*N+(i+1)%N;f.append((a,b,b+N,a+N))
 f.extend([tuple(reversed(range(N))),tuple((len(profile)-1)*N+i for i in range(N))]);return mesh(n,v,f,mat)
def cyl(n,p,r,h,mat,axis=(0,0,1),N=24):
 bpy.ops.mesh.primitive_cylinder_add(vertices=N,radius=r,depth=h,location=p);o=bpy.context.object;o.name=n;o.rotation_euler=Vector(axis).to_track_quat('Z','Y').to_euler();add(o,mat)
 for f in o.data.polygons:f.use_smooth=len(f.vertices)==4
 return o
def sphere(n,p,s,mat):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,location=p);o=bpy.context.object;o.name=n;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);add(o,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o
def box(n,p,s,mat,b=.003):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=n;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);add(o,mat)
 if b:
  m=o.modifiers.new('Rounded formed edge','BEVEL');m.width=b;m.segments=2;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=m.name)
  for f in o.data.polygons:f.use_smooth=True
  m=o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');m.keep_sharp=True;bpy.ops.object.modifier_apply(modifier=m.name)
 return o
def tube(n,pts,r,mat,closed=False):
 cu=bpy.data.curves.new(n,'CURVE');cu.dimensions='3D';cu.resolution_u=1 if len(pts)>16 else 5;cu.bevel_depth=r;cu.bevel_resolution=1;sp=cu.splines.new('BEZIER');sp.bezier_points.add(len(pts)-1)
 for j,(b,p) in enumerate(zip(sp.bezier_points,pts)):
  p=Vector(p);prev=Vector(pts[(j-1)%len(pts)] if closed or j>0 else pts[j]);nxt=Vector(pts[(j+1)%len(pts)] if closed or j<len(pts)-1 else pts[j]);t=(nxt-prev)/6;left=p-t;right=p+t
  for k in range(3):left[k]=max(min(prev[k],p[k]),min(max(prev[k],p[k]),left[k]));right[k]=max(min(nxt[k],p[k]),min(max(nxt[k],p[k]),right[k]))
  b.co=p;b.handle_left_type='FREE';b.handle_right_type='FREE';b.handle_left=left;b.handle_right=right
 sp.use_cyclic_u=closed;o=bpy.data.objects.new(n,cu);bpy.context.collection.objects.link(o);bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.convert(target='MESH');o=bpy.context.object;add(o,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o
def rod(n,a,b,r,mat):d=Vector(b)-Vector(a);return cyl(n,(Vector(a)+Vector(b))/2,r,d.length,mat,d)
def ring(n,r,z,thick,mat):return tube(n,[(r*cos(a),r*sin(a),z) for a in [i*2*pi/32 for i in range(32)]],thick,mat,True)
def finish_group(n,pivot=(0,0,0)):
 global parts
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=n;scene.cursor.location=pivot;bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.1519,island_margin=.012,scale_to_bounds=True);bpy.ops.object.mode_set(mode='OBJECT');o.data.uv_layers.active.name='UVMap';groups.append(o);parts=[];return o


roots=[];assets={}
def export_asset(name,markers,notes,animation=None):
 global groups
 root=bpy.data.objects.new('EP_IC_'+name,None);bpy.context.collection.objects.link(root);roots.append(root)
 for o in groups:o.parent=root
 anchors={'ROOT':(0,0,0),'AXIS_X':(.1,0,0),'AXIS_Y':(0,.1,0),'AXIS_Z':(0,0,.1),'FRONT_BASE':(0,0,0),'FRONT_TIP':(0,-.1,0)};anchors.update(markers);full={}
 for n,p in anchors.items():
  fn=name+'__'+n;o=bpy.data.objects.new(fn,None);bpy.context.collection.objects.link(o);o.parent=root;o.location=p;o.empty_display_type='PLAIN_AXES';o.empty_display_size=.022;full[fn]=list(p)
 bpy.context.view_layer.update();bpy.ops.object.select_all(action='DESELECT')
 for o in [root]+list(root.children_recursive):o.select_set(True)
 bpy.context.view_layer.objects.active=root;relative='exports/EP_IC_'+name+'.fbx';bpy.ops.export_scene.fbx(filepath=os.path.join(BASE,relative),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False)
 coords=[o.matrix_world@v.co for o in groups for v in o.data.vertices];bb=[[min(p[i] for p in coords),max(p[i] for p in coords)] for i in range(3)]
 assets[name]={'file':relative,'bounds_blender_xyz':bb,'dimensions_width_depth_height':[b-a for a,b in bb],'materials':sorted({m.name for o in groups for m in o.data.materials}),'mesh_objects':[{'name':o.name,'vertices':len(o.data.vertices),'triangles':sum(len(f.vertices)-2 for f in o.data.polygons),'uv_layers':[u.name for u in o.data.uv_layers],'pivot_blender_xyz':list(o.matrix_world.translation)} for o in groups],'anchors_blender_xyz':full,'notes':notes}
 if animation:assets[name]['animation']=animation
 for o in [root]+list(root.children_recursive):o.hide_set(True);o.hide_render=True
 groups=[];print('EXPORTED',name,assets[name]['dimensions_width_depth_height'],flush=True)

PIVOT=Vector((-.12,-.150,-.080));LEVER_RADIUS=.300;REST=math.radians(-35)
def lp(r,angle=REST,y=None):
 return PIVOT+Vector((r*cos(angle),(y-PIVOT.y) if y is not None else 0,-r*sin(angle)))
high=lp(LEVER_RADIUS);isolated=lp(LEVER_RADIUS,math.radians(35))

# Folded steel enclosure. A seam, hinges and captive screws explain access.
box('Folded steel body',(0,.005,0),(.500,.140,.650),'IC_Enamel',.010)
box('Door dark seal',(0,-.067,0),(.483,.010,.631),'IC_Rubber',.006)
box('Raised access cover',(0,-.080,0),(.480,.017,.628),'IC_DoorEnamel',.008)
for z in [-.194,.194]:
 box('Hinge leaf',(-.243,-.035,z),(.016,.052,.079),'IC_Enamel',.003)
 cyl('Hinge barrel',(-.250,-.049,z),.010,.091,'IC_EdgeMetal',N=16)
for x,z in [(.211,.280),(.211,-.280),(-.211,-.280)]:
 cyl('Cover fastener',(x,-.091,z),.006,.004,'IC_EdgeMetal',(0,-1,0),16)
 box('Fastener slot',(x,-.0935,z),(.007,.0006,.001),'IC_DarkMetal',.0002)
# Small enamel damage at exposed corners; physical geometry, no noise texture.
for x,z,w in [(.235,.291,.007),(-.222,.304,.012),(.221,-.303,.010)]:box('Exposed cover edge',(x,-.089,z),(w,.001,.002),'IC_EdgeMetal',.0003)
# Blank state tabs deliberately avoid new words, numerals or data.
for x,z in [(.104,.131),(-.094,-.280)]:
 box('State plate holder',(x,-.095,z),(.213,.013,.040),'IC_DarkMetal',.002)
 box('Blank state inset',(x,-.102,z),(.192,.002,.024),'IC_DialIvory',.001)
box('Blank service tag',(.117,-.091,.255),(.165,.003,.056),'IC_DarkMetal',.002)
# Selector mounting plate / spindle. The control is independent of the gauge.
cyl('Selector flange',(-.12,-.097,-.08),.086,.014,'IC_DarkMetal',(0,-1,0),40)
cyl('Selector shaft seal',(-.12,-.109,-.08),.044,.015,'IC_Rubber',(0,-1,0),32)
cyl('Selector spindle',(-.12,-.132,-.08),.025,.045,'IC_EdgeMetal',(0,-1,0),32)
for angle in [135,225]:
 a=math.radians(angle);cyl('Mount plate bolt',(-.12+.065*cos(a),-.106,-.08+.065*sin(a)),.005,.005,'IC_EdgeMetal',(0,-1,0),16)
# A moving dog at R=.061 is bounded by two fixed pins. No decorative wheel.
for a in [-51,51]:
 p=lp(.067,math.radians(a),-.135);cyl('Physical travel stop',p,.008,.040,'IC_EdgeMetal',(0,-1,0),20)
box('Bottom cable gland boss',(.090,.025,-.326),(.082,.061,.028),'IC_DarkMetal',.004)
cyl('Cable gland',(.090,.025,-.351),.023,.034,'IC_Bakelite',N=20)
finish_group('IsolationHousing')

# The moving group is authored in MARCHA, with zero rotation and exact origin.
cyl('Moving selector hub',PIVOT,.033,.023,'IC_Bakelite',(0,-1,0),32)
rod('Selector arm',lp(.022,y=-.150),lp(.243,y=-.150),.011,'IC_EdgeMetal')
grip=box('Moulded bakelite grip',tuple(lp(.263,y=-.158)),(.116,.047,.041),'IC_Bakelite',.010);grip.rotation_euler.y=REST
for r in [.216,.308]:
 cap=box('Grip mould seam',tuple(lp(r,y=-.158)),(.002,.048,.042),'IC_DarkMetal',.0007);cap.rotation_euler.y=REST
dog=box('Selector stop dog',tuple(lp(.061,y=-.138)),(.018,.025,.012),'IC_DarkMetal',.002);dog.rotation_euler.y=REST
cyl('Selector hub centre',PIVOT+Vector((0,-.014,0)),.010,.005,'IC_EdgeMetal',(0,-1,0),20)
lever=finish_group('IsolationLever',tuple(PIVOT))
export_asset('IsolationPanel',{'LEVER_PIVOT':tuple(PIVOT),'LEVER_AXIS_POS':tuple(PIVOT+Vector((0,.1,0))),'LEVER_GRIP_MARCHA':tuple(high),'LEVER_GRIP_AISLADO':tuple(isolated),'LABEL_MARCHA':(.104,-.104,.131),'LABEL_AISLADO':(-.094,-.104,-.280),'SERVICE_LABEL':(.117,-.094,.255),'GAUGE_MOUNT':(-.120,-.086,.203),'BACK_MOUNT':(0,.075,-.23),'CABLE_PORT':(.090,.025,-.368)},'Root is the control centre; place at world height1.5m. Enclosure approximately .5x.163x.65m; handle and fittings extend in front/below. IsolationHousing is static; animate only IsolationLever. Export rest is MARCHA, with zero rotation and origin at LEVER_PIVOT. Gauge is a separate export and mounts at GAUGE_MOUNT; no labels, lamps, text, sounds, colliders or baked animation exported.',{'mesh':'IsolationLever','pivot_blender_xyz':list(PIVOT),'axis_blender_xyz':[0,1,0],'rest_state':'MARCHA','marcha_degrees':0,'aislado_degrees':70,'suggested_duration_seconds':1.8,'grip_marcha_blender_xyz':list(high),'grip_aislado_blender_xyz':list(isolated),'interpolation':'Lerp scalar angle 0 to70, then apply axis-angle relative to rest. Check grip anchors after Unity corrected mirrored import; do not infer signs from Transform.forward.'})

# Dedicated pedestal, no broad altar/plinth. Feet meet floor at rootZ-1.5.
box('Pedestal sole',(0,.055,-1.487),(.400,.330,.026),'IC_DarkMetal',.006)
box('Pedestal tube',(0,.058,-.865),(.088,.083,1.218),'IC_Enamel',.005)
box('Pedestal neck flange',(0,.071,-.312),(.240,.078,.016),'IC_EdgeMetal',.003)
box('Rear vertical mount',(0,.0875,-.198),(.160,.027,.220),'IC_Enamel',.004)
for x in [-.055,.055]:
 cyl('Panel rear fastening',(x,.1015,-.151),.006,.006,'IC_EdgeMetal',(0,1,0),16)
for x in [-.153,.153]:
 for y in [-.062,.174]:
  cyl('Base washer',(x,y,-1.470),.014,.004,'IC_EdgeMetal',N=16)
  cyl('Foundation bolt',(x,y,-1.465),.008,.008,'IC_DarkMetal',N=6)
# Two formed gussets visibly connect the tube to the base.
for x in [-.050,.050]:
 vs=[(x-.006,-.068,-1.472),(x-.006,.015,-1.472),(x-.006,.015,-1.304),(x+.006,-.068,-1.472),(x+.006,.015,-1.472),(x+.006,.015,-1.304)]
 mesh('Pedestal gusset',vs,[(0,2,1),(3,4,5),(0,1,4,3),(1,2,5,4),(2,0,3,5)],'IC_Enamel',False)
finish_group('PedestalStructure')
tube('Lower flexible conduit',[(.09,.025,-.364),(.108,.025,-.396),(.119,.075,-.47),(.106,.111,-.58),(.061,.109,-.66),(.062,.109,-1.20),(.068,.111,-1.378),(.059,.108,-1.470)],.010,'IC_Rubber')
for z in [-.69,-1.20]:box('Conduit saddle',(.056,.110,z),(.044,.018,.017),'IC_EdgeMetal',.002)
finish_group('PedestalConduit')
export_asset('ControlPedestal',{'FLOOR_CONTACT':(0,0,-1.5),'PANEL_CENTRE':(0,0,0),'BACK_MOUNT':(0,.075,-.23),'CABLE_PORT':(.090,.025,-.368)},'Root matches IsolationPanel centre, not floor. Install both with same transform; floor at sourceZ-1.500. Base .40x.33m. Support and routed conduit are separate meshes. No decorative control, switches, words or lights.')

# Original pressure gauge: independent case, engraved-like ticks, visible needle.
# Source origin is gauge centre, back plane lies Y0 at panel mount face.
cyl('Pressure casing',(0,-.031,0),.099,.068,'IC_Enamel',(0,-1,0),64)
cyl('Rear mounting flange',(0,.002,0),.087,.010,'IC_DarkMetal',(0,-1,0),48)
cyl('Ivory pressure face',(0,-.067,0),.090,.003,'IC_DialIvory',(0,-1,0),64)
# Spun bezel is an open annulus, so it never covers the face or needle.
profiles=[(.098,-.057),(.104,-.063),(.104,-.074),(.100,-.080),(.092,-.080),(.090,-.074),(.092,-.066)];N=64
v=[(r*cos(i*2*pi/N),y,r*sin(i*2*pi/N)) for r,y in profiles for i in range(N)];f=[]
for j in range(len(profiles)):
 for i in range(N):a=j*N+i;b=j*N+(i+1)%N;c=((j+1)%len(profiles))*N;f.append((a,b,c+(i+1)%N,c+i))
mesh('Rolled pressure bezel',v,f,'IC_EdgeMetal')
# Scale is 270 degrees, zero lower-left and high lower-right, no invented units.
for i in range(37):
 a=math.radians(225-i*7.5);major=i%4==0;length=.014 if major else .007;r=.078-length/2
 tick=box('Pressure graduation',(r*cos(a),-.0692,r*sin(a)),(length,.0012,.0023 if major else .0013),'IC_DialInk',0);tick.rotation_euler.y=-a
box('Blank gauge unit zone',(0,-.0693,-.046),(.034,.0006,.012),'IC_DialIvory',0)
cyl('Gauge lower connection',(0,-.013,-.111),.016,.033,'IC_Brass',N=12)
cyl('Gauge threaded neck',(0,-.013,-.133),.010,.015,'IC_Brass',N=20)
tube('Pressure impulse return',[(0,-.013,-.139),(0,-.013,-.151),(0,-.008,-.160),(0,.006,-.160),(0,.022,-.160)],.005,'IC_Brass')
finish_group('PressureGaugeHousing')
NP=Vector((0,-.073,0));NA=math.radians(-45);NR=.070
# A tapered stamped needle plus counterweight, all in one mesh with true pivot.
points=[(-.018,-.004),(-.018,.004),(.013,.0032),(.070,.0008),(.071,0),(.070,-.0008),(.013,-.0032)]
v=[]
for depth in [-.001,.001]:
 for x,z in points:
  v.append(tuple(NP+Vector((x*cos(NA)-z*sin(NA),depth,x*sin(NA)+z*cos(NA)))))
L=len(points);f=[tuple(reversed(range(L))),tuple(L+i for i in range(L))]
for i in range(L):j=(i+1)%L;f.append((i,j,j+L,i+L))
mesh('Stamped pressure needle',v,f,'IC_Needle',False)
cyl('Needle centre cap',NP+Vector((0,-.002,0)),.007,.004,'IC_DialInk',(0,-1,0),24)
needle=finish_group('PressureNeedle',tuple(NP))
NH=NP+Vector((NR*cos(NA),0,NR*sin(NA)));NZ=NP+Vector((NR*cos(math.radians(225)),0,NR*sin(math.radians(225))))
export_asset('PressureGauge',{'NEEDLE_PIVOT':tuple(NP),'NEEDLE_AXIS_POS':tuple(NP+Vector((0,.1,0))),'NEEDLE_TIP_HIGH':tuple(NH),'NEEDLE_TIP_ZERO':tuple(NZ),'FACE_CENTRE':(0,-.0685,0),'BACK_MOUNT':(0,0,0),'PRESSURE_PORT':(0,.022,-.160),'UNIT_LABEL':(0,-.070,-.046)},'Root at gauge face centre in XZ, mounting planeY0. Front-Y, no glass pane obscures the moving needle. PressureGaugeHousing static, PressureNeedle separate with exact pivot. Rest export is HIGH, blade down/right45deg. ZERO is down/left45deg after270deg across the upper scale. No words/numbers/units, labels, collider, light or animation.',{'mesh':'PressureNeedle','pivot_blender_xyz':list(NP),'axis_blender_xyz':[0,1,0],'rest_state':'HIGH','high_degrees':0,'zero_degrees':-270,'suggested_duration_seconds':3,'needle_tip_high_blender_xyz':list(NH),'needle_tip_zero_blender_xyz':list(NZ),'interpolation':'Interpolate scalar angle 0 to-270 then AngleAxis relative to rest. Do not Slerp endpoint quaternions, which traverses the lower90deg gap. Read actual machine pressure, never Echo activity/hold progress.'})

triangles=sum(sum(m['triangles'] for m in a['mesh_objects']) for a in assets.values())
manifest={'date':'2026-10-01','authorship':'Original project geometry and PBR values. No external model, image, texture, font, service or add-on. No exported words, screen, light, sound or animation. All visible studio surfaces are the exported material values.','axis_contract':'Source Xwidth,-Yfront,Zup. FBX -Z/+Y. Observed Unity importer maps(-X,Z,-Y): negate imported root localScale.x once, retain imported magnitudes/rotation, then placementRotation*importedRotation. Corrected map(X,Z,-Y), nominal frontUnity+Z. Direction markers require world-position differences; endpoint anchors are authoritative for animation signs.','materials':VALUES,'total_triangles':triangles,'assets':assets,'assembly':{'root_world_height_m':1.5,'IsolationPanel':[0,0,0],'ControlPedestal':[0,0,0],'PressureGauge':[-.120,-.086,.203]},'verification_limit':'Blender studio previews and roundtrip only. Unity motion/collision, interaction clarity, timing and human judgment remain integration/playtest work.'}
json.dump(manifest,open(os.path.join(BASE,'manifest.json'),'w'),indent=2)
for root in roots:
 for o in [root]+list(root.children_recursive):o.hide_set(False);o.hide_render=False
for i,root in enumerate(roots):root.location.x=i*1.1
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'EchoIsolationControl.blend'))
for root in roots:root.location=(0,0,0)

# Studio previews use no non-exported surface or signage. Source save is clean.
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True;scene.render.resolution_x=1000;scene.render.resolution_y=1100;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX';scene.world.color=(.06,.06,.06)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.002));ground=bpy.context.object;m=bpy.data.materials.new('Preview floor');m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.09,.10,.087,1);m.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.88;ground.data.materials.append(m)
for n,p,power,size,col in [('Key',(-2,-3,4),420,3,(1,.89,.73)),('Fill',(3,-1,2.5),280,3,(.68,.82,1)),('Rim',(0,3,3),330,3,(1,.94,.82))]:
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.size=size;d.color=col;o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,0,1.1))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam
def show(ids):
 for i,root in enumerate(roots):
  for o in [root]+list(root.children_recursive):o.hide_set(i not in ids);o.hide_render=i not in ids
def render(n,p,target,lens=52):
 cam.location=p;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=lens;scene.render.filepath=os.path.join(PRE,n+'.png');bpy.ops.render.render(write_still=True)
show([0,1,2]);roots[0].location.z=1.5;roots[1].location.z=1.5;roots[2].location=(-.12,-.086,1.703)
render('Assembly-MARCHA',(-1.42,-2.55,1.76),(0,0,.94),48)
scene.render.resolution_x=1280;scene.render.resolution_y=720;cam.data.sensor_fit='VERTICAL';cam.data.sensor_height=24
render('Player-1_5m-MARCHA',(0,-1.5,1.63),(0,0,1.50),24/(2*math.tan(math.radians(30))))
lever.rotation_euler.y=math.radians(70);needle.rotation_euler.y=math.radians(-270)
render('Player-1_5m-AISLADO',(0,-1.5,1.63),(0,0,1.50),24/(2*math.tan(math.radians(30))))
scene.render.resolution_x=1000;scene.render.resolution_y=1100;cam.data.sensor_fit='AUTO'
render('Assembly-AISLADO',(-1.42,-2.55,1.76),(0,0,.94),48)
lever.rotation_euler.y=0;needle.rotation_euler.y=0
show([0,2]);render('Control-front',(0,-1.12,1.54),(0,-.08,1.52),52)
show([2]);roots[2].location=(0,0,.24);render('Gauge-HIGH',(.0,-.53,.28),(0,-.03,.23),62)
needle.rotation_euler.y=math.radians(-270);render('Gauge-ZERO',(.0,-.53,.28),(0,-.03,.23),62);needle.rotation_euler.y=0
show([0,1]);render('Assembly-rear',(1.20,2.3,1.67),(0,0,.94),48)
print('ISOLATION CONTROL COMPLETE',triangles,flush=True)
