"""Original static adult right hand and jacket sleeve gripping the reserve handle.
Metres; source X width / -Y front / Z up. No external media, rig or animation.
ReserveUnit is read only, imported in memory after source save for preview only.
"""
import bpy, bmesh, json, math, hashlib
from pathlib import Path
from mathutils import Vector, Matrix
from math import sin,cos,pi
BASE=Path(__file__).resolve().parent
for p in [BASE/'exports',BASE/'previews']:p.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
VALUES={'FIG_Skin':{'color':[.37,.255,.182,1],'roughness':.65,'metallic':0},
        'FIG_Jacket':{'color':[.11,.15,.14,1],'roughness':.86,'metallic':0},
        'FIG_Stitch':{'color':[.060,.078,.070,1],'roughness':.89,'metallic':0},
        'RG_Nail':{'color':[.40,.29,.225,1],'roughness':.52,'metallic':0}}
mats={}
for name,d in VALUES.items():
 m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=d['color'];m.roughness=d['roughness'];m.metallic=d['metallic'];p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=d['color'];p.inputs['Roughness'].default_value=d['roughness'];p.inputs['Metallic'].default_value=d['metallic'];mats[name]=m

def sphere(name,point,scale):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=28,ring_count=16,location=point);o=bpy.context.object;o.name=name;o.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return o

def tube(name,points,radii,sides=18,substeps=4,fold=False,section_power=1):
 # Catmull-Rom centreline with interpolated oval sections, not equal-radius capsules.
 centers=[];rads=[]
 for k in range(len(points)-1):
  p0=Vector(points[max(0,k-1)]);p1=Vector(points[k]);p2=Vector(points[k+1]);p3=Vector(points[min(len(points)-1,k+2)])
  for j in range(substeps):
   t=j/substeps;t2=t*t;t3=t2*t
   centers.append(.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t2+(-p0+3*p1-3*p2+p3)*t3))
   rads.append(tuple(radii[k][i]*(1-t)+radii[k+1][i]*t for i in range(2)))
 centers.append(Vector(points[-1]));rads.append(radii[-1]);vertices=[]
 for j,(p,r) in enumerate(zip(centers,rads)):
  tangent=(centers[min(j+1,len(centers)-1)]-centers[max(0,j-1)]).normalized()
  side=Vector((1,0,0));side=(side-tangent*side.dot(tangent)).normalized();other=tangent.cross(side).normalized()
  for i in range(sides):
   a=i*2*pi/sides
   # Oval pads flatten the contact-facing skin; sleeve has broad low cloth folds.
   warp=1
   if fold:
    t=j/(len(centers)-1)
    warp+=.046*sin(t*31+2.5*sin(a))*sin(pi*t)**2+.025*cos(3*a+13*t)
   vertices.append(tuple(p+side*(r[0]*math.copysign(abs(cos(a))**section_power,cos(a))*warp)+other*(r[1]*math.copysign(abs(sin(a))**section_power,sin(a))*warp)))
 faces=[]
 for j in range(len(centers)-1):
  for i in range(sides):a=j*sides+i;b=j*sides+(i+1)%sides;faces.append((a,b,b+sides,a+sides))
 faces.extend([tuple(reversed(range(sides))),tuple((len(centers)-1)*sides+i for i in range(sides))])
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.update();obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);return obj

def select_only(objs):
 bpy.ops.object.select_all(action='DESELECT')
 for o in objs:o.select_set(True)
 bpy.context.view_layer.objects.active=objs[0]
def finish(obj,name,material):
 obj.name=name;obj.data.materials.clear();obj.data.materials.append(mats[material]);select_only([obj]);scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
 for face in obj.data.polygons:face.use_smooth=True
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.uv.smart_project(angle_limit=1.0,island_margin=.009,scale_to_bounds=True);bpy.ops.object.mode_set(mode='OBJECT');obj.data.uv_layers.active.name='UVMap'
 return obj

# Dorsal palm transitions continuously into the wrist. Index is source -X and
# thumb opposes it on that side: this is the adult right hand, palm downward.
skin=[sphere('Palm dorsal volume',(.003,-.021,.236),(.039,.041,.016)),
      sphere('Thenar pad',(-.027,-.030,.225),(.017,.024,.015)),
      sphere('Continuous thumb web',(-.029,-.024,.219),(.014,.020,.013)),
      sphere('Ulnar palm pad',(.027,-.020,.230),(.016,.032,.020)),
      tube('Wrist continuity',[(.013,-.043,.235),(.024,-.062,.235),(.041,-.082,.237),(.061,-.108,.239)],[(.028,.013),(.023,.0125),(.024,.015),(.026,.019)],24,6)]
# Four tapered flexed fingers. Fused bases form a palm, while small inter-finger
# grooves preserve count. The distal pads support the handle from below.
for i,(x,width,reach) in enumerate([(-.029,.0078,0),(-.009,.0085,.003),(.012,.0080,.0015),(.030,.0066,-.002)]):
 pts=[(x,.003,.238),(x,.019+reach,.234),(x,.027+reach,.219),(x,.023+reach,.200),(x,.011,.184),(x,-.006,.183)]
 skin.append(tube('Flexed finger '+str(i+1),pts,[(width,.010),(width*1.02,.0105),(width,.009),(width*.93,.0083),(width*.86,.0077),(width*.65,.0058)],18,7,section_power=.72))
 skin.append(sphere('Distal pad '+str(i+1),pts[-1],(width*.62,.006,.006)))
# Opposed thumb, deliberately asymmetric; its pad closes against index rather
# than forming a fifth parallel cylinder. No articulated fingertip channels.
skin.append(tube('Opposed thumb',[(-.023,-.043,.230),(-.039,-.029,.223),(-.043,-.012,.217),(-.034,-.018,.205),(-.017,-.018,.201)],[(.014,.015),(.013,.013),(.012,.011),(.010,.009),(.0065,.007)],20,7))
skin.append(sphere('Thumb pad',(-.017,-.018,.201),(.007,.007,.007)))
select_only(skin);bpy.ops.object.join();hand=bpy.context.object
# A watertight, joined organic surface eliminates primitive intersections.
hand.data.remesh_voxel_size=.0007;hand.data.remesh_voxel_adaptivity=0;bpy.ops.object.voxel_remesh()
smooth=hand.modifiers.new('Subtle skin continuity','SMOOTH');smooth.factor=.34;smooth.iterations=7;bpy.ops.object.modifier_apply(modifier=smooth.name)
hand.data.calc_loop_triangles();ratio=min(1,5100/max(1,len(hand.data.loop_triangles)))
dec=hand.modifiers.new('Fixed first-person budget','DECIMATE');dec.ratio=ratio;bpy.ops.object.modifier_apply(modifier=dec.name)
# Smooth the web and wrist after reduction: low-resolution crease spikes must
# not become black slits under the game's directional lighting.
soft=hand.vertex_groups.new(name='Soft tissue transitions')
for v in hand.data.vertices:
 w=hand.matrix_world@v.co
 weight=max(math.exp(-((w-Vector((-.029,-.024,.220))).length/.023)**2),
            math.exp(-((w-Vector((.018,-.043,.235))).length/.026)**2))
 if weight>.01:soft.add([v.index],weight,'REPLACE')
polish=hand.modifiers.new('Thumb web and tapered wrist polish','SMOOTH');polish.factor=.55;polish.iterations=14;polish.vertex_group=soft.name
bpy.ops.object.modifier_apply(modifier=polish.name)
# Remove remesh/decimation flecks, keep the single connected hand only.
bm=bmesh.new();bm.from_mesh(hand.data);unseen=set(bm.verts);components=[]
while unseen:
 todo=[unseen.pop()];component=set(todo)
 while todo:
  for edge in todo.pop().link_edges:
   for v in edge.verts:
    if v in unseen:unseen.remove(v);component.add(v);todo.append(v)
 components.append(component)
largest=max(components,key=len)
bmesh.ops.delete(bm,geom=[v for c in components if c is not largest for v in c],context='VERTS')
# Skin pads meet the grip circumference without passing through its cylinder.
for v in bm.verts:
 world=hand.matrix_world @ v.co
 if abs(world.x)<.040:
  radial=Vector((0,world.y,world.z-.201));r=radial.length
  if 0<r<.0115:
   radial*=.0115/r;world.y=radial.y;world.z=.201+radial.z
   v.co=hand.matrix_world.inverted() @ world
bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-7)
bmesh.ops.dissolve_degenerate(bm,edges=list(bm.edges),dist=1e-7)
bmesh.ops.triangulate(bm,faces=list(bm.faces));bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
bm.to_mesh(hand.data);bm.free()
hand=finish(hand,'ReserveRightHand','FIG_Skin')
# The sleeve encloses the wrist, extends toward source screen-right/front after corrected carry placement, and widens
# slightly toward the unseen forearm. A short fabric cuff explains the junction.
sleeve=tube('Sleeve',[(.037,-.075,.237),(.043,-.083,.238),(.048,-.092,.239),(.053,-.100,.240),(.060,-.109,.240),(.083,-.139,.236),(.112,-.176,.225),(.151,-.220,.209)],[(.0265,.0205),(.028,.0218),(.029,.023),(.0285,.023),(.030,.025),(.034,.029),(.037,.031),(.041,.034)],28,6,True)
sleeve=finish(sleeve,'ReserveJacketSleeve','FIG_Jacket')
sleeve.data.materials.append(mats['FIG_Stitch'])
for face in sleeve.data.polygons:
 # Continuous geometry with cuff material for first three segments only.
 if face.index < 3*6*28:face.material_index=1
nail=sphere('Thumb nail',(-.024,-.027,.205),(.008,.001,.0055))
nail.rotation_euler.y=-.35
select_only([nail]);dec=nail.modifiers.new('Small nail budget','DECIMATE');dec.ratio=.22;bpy.ops.object.modifier_apply(modifier=dec.name)
nail=finish(nail,'ReserveThumbNail','RG_Nail')
objs=[hand,sleeve,nail]
# Final authored carry orientation: source -X is Player screen-right after the
# common corrected importer and 180-degree carried placement. Mirror geometry
# here (not a negative source-object scale), including opposed thumb anatomy.
for obj in objs:
 for vertex in obj.data.vertices:vertex.co.x *= -1
 select_only([obj]);bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT')
root=bpy.data.objects.new('EP_ReserveGrip',None);bpy.context.collection.objects.link(root)
for obj in objs:obj.parent=root
anchors={'ROOT':(0,0,0),'AXIS_X':(.1,0,0),'AXIS_Y':(0,.1,0),'AXIS_Z':(0,0,.1),'HANDLE_CENTRE':(0,0,.201),'WRIST_CENTRE':(-.039,-.079,.238),'SLEEVE_EXIT':(-.151,-.220,.209)}
for n,p in anchors.items():
 o=bpy.data.objects.new('ReserveGrip__'+n,None);bpy.context.collection.objects.link(o);o.parent=root;o.location=p;o.empty_display_size=.015
select_only([root]+list(root.children_recursive));bpy.context.view_layer.update()
file='exports/EP_ReserveGrip.fbx';bpy.ops.export_scene.fbx(filepath=str(BASE/file),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False)
coords=[o.matrix_world@v.co for o in objs for v in o.data.vertices];bounds=[[min(v[i] for v in coords),max(v[i] for v in coords)] for i in range(3)]
meshes=[]
for o in objs:
 o.data.calc_loop_triangles();meshes.append({'name':o.name,'vertices':len(o.data.vertices),'triangles':len(o.data.loop_triangles),'uv_layers':[layer.name for layer in o.data.uv_layers],'pivot_blender_xyz':list(o.matrix_world.translation)})
asset={'file':file,'bounds_blender_xyz':bounds,'mesh_objects':meshes,'materials':list(VALUES),'anchors_blender_xyz':{'ReserveGrip__'+k:list(v) for k,v in anchors.items()},'notes':'Static adult right hand and short jacket sleeve only. Common origin matches original ReservePack bottom. No pack geometry, armature, animation, light, collider or Echo data. Source hand grip centre is(0,0,.201), source sleeve exits-X/-Y.'}
manifest={'date':'2026-10-01','authorship':'Original project-authored procedural skin topology and cloth surfaces. No third-party or generated bitmap/model. Material values match OverhaulFigure/build_overhaul_figure.py FIG_Skin/FIG_Jacket/FIG_Stitch; those original project assets are unchanged.','axis_contract':'Metres; source X width,-Yfront,Zup. FBX-Z/+Y. Unity common corrected import with ReserveUnit (negate imported rootX once; retain imported rotation/scale). Endpoints are common world-source coordinates, not animation.','materials':VALUES,'total_triangles':sum(m['triangles'] for m in meshes),'assets':{'ReserveGrip':asset},'preview_only_pack_reference':'../ReserveUnit/exports/EP_RU_ReservePack.fbx','preview_pack_sha256':hashlib.sha256((BASE.parent/'ReserveUnit/exports/EP_RU_ReservePack.fbx').read_bytes()).hexdigest(),'verification_limit':'Source and FBX/studio checks only until integrated. Human anatomy/comfort, in-game camera clipping, collision masking and visual approval are not established by numerical checks.'}
(BASE/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
bpy.ops.wm.save_as_mainfile(filepath=str(BASE/'EchoReserveGrip.blend'))
print('GRIP EXPORTED',manifest['total_triangles'],flush=True)
# External pack is used only in this transient preview scene, after source save.
bpy.ops.import_scene.fbx(filepath=str(BASE.parent/'ReserveUnit/exports/EP_RU_ReservePack.fbx'))
preview_pack_roots=[o for o in bpy.context.selected_objects if o.parent is None]
scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True;scene.render.resolution_x=1200;scene.render.resolution_y=1000;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX';scene.world.color=(.055,.055,.055)
for n,p,power,size,col in [('Key',(-.4,-.45,.8),28,.7,(1,.91,.79)),('Fill',(.4,-.2,.55),9,.5,(.70,.82,1)),('Rim',(0,.5,.65),20,.5,(1,.94,.82))]:
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.size=size;d.color=col;o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,0,.19))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam;cam.data.lens=58
for name,p,target,lens in [('assembled-front',(-.30,-.52,.40),(.030,-.065,.145),58),('grip-close',(-.13,-.30,.37),(.008,-.025,.215),75),('assembled-back',(.15,.46,.39),(.015,-.060,.153),62),('carried-frame',(0,-.46,.41),(-.11,.04,.35),36)]:
 if name!='carried-frame':p=(-p[0],p[1],p[2]);target=(-target[0],target[1],target[2])
 if name=='carried-frame':
  scene.render.resolution_x=1280;scene.render.resolution_y=720;cam.data.sensor_fit='VERTICAL';cam.data.sensor_height=24;lens=24/(2*math.tan(math.radians(60)/2))
  # In Blender camera +Y view/+Zup, this is Unity carry(.26,-.39,.55),
  # corrected source(X,Z,-Y), then UnityEuler(8,170,-8), uniform scale1.15.
  source_to_unity=Matrix(((1,0,0,0),(0,0,1,0),(0,-1,0,0),(0,0,0,1)))
  unity_to_blender=Matrix(((1,0,0,0),(0,0,1,0),(0,1,0,0),(0,0,0,1)))
  placement=Matrix.Rotation(math.radians(170),4,'Y') @ Matrix.Rotation(math.radians(8),4,'X') @ Matrix.Rotation(math.radians(-8),4,'Z')
  carried=Matrix.Translation((.26,.55,-.39)) @ unity_to_blender @ placement @ source_to_unity @ Matrix.Diagonal((1.15,1.15,1.15,1))
  root.matrix_world=carried
  for pack_root in preview_pack_roots:pack_root.matrix_world=carried @ pack_root.matrix_world
  p=(0,0,0);target=(0,1,0)
 cam.location=p;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=lens;scene.render.filepath=str(BASE/'previews'/f'{name}.png');bpy.ops.render.render(write_still=True)
# A low, oblique warm source exposes pinched shading in the actual carried pose.
for light in [o for o in scene.objects if o.type=='LIGHT']:
 light.data.energy=.15
key=bpy.data.objects['Key'];key.location=(.04,.08,.22);key.data.energy=5;key.data.size=.12;key.data.color=(1,.75,.48)
key.rotation_euler=(Vector((.27,.53,-.13))-key.location).to_track_quat('-Z','Y').to_euler()
scene.world.color=(.008,.008,.008)
scene.render.filepath=str(BASE/'previews'/'carried-raking.png');bpy.ops.render.render(write_still=True)
print('GRIP PREVIEWS COMPLETE',flush=True)
