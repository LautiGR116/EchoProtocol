"""Original small metric kit: water path, paving, drainage. No outside assets.
Only writes this directory. Source X length, -Y away from wall, Z up.
"""
import bpy, bmesh, math, json, hashlib
from pathlib import Path
from mathutils import Vector
BASE=Path(__file__).resolve().parent
OUT=BASE/'exports'; PRE=BASE/'previews'
OUT.mkdir(exist_ok=True); PRE.mkdir(exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene; scene.unit_settings.system='METRIC'; scene.unit_settings.scale_length=1
MATERIALS={'CY_DullSteel':{'color':(.14,.165,.15,1),'roughness':.66,'metallic':.60},
           'CY_Mineral':{'color':(.31,.33,.305,1),'roughness':.88,'metallic':0}}
mats={}
for name,d in MATERIALS.items():
 m=bpy.data.materials.new(name); m.use_nodes=True; m.diffuse_color=d['color']
 p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=d['color']; p.inputs['Roughness'].default_value=d['roughness']; p.inputs['Metallic'].default_value=d['metallic']; mats[name]=m
parts=[]; assets={}; asset_objects={}
def register(o,mat):
 o.data.materials.append(mats[mat]); parts.append(o); return o
def fix_normals(o):
 bm=bmesh.new(); bm.from_mesh(o.data); bmesh.ops.recalc_face_normals(bm,faces=bm.faces); bm.to_mesh(o.data); bm.free()
def bevel(o,width,segments=2):
 mod=o.modifiers.new('Small manufactured edge','BEVEL'); mod.width=width; mod.segments=segments
 bpy.context.view_layer.objects.active=o; bpy.ops.object.modifier_apply(modifier=mod.name)
 for p in o.data.polygons:p.use_smooth=True
 mod=o.modifiers.new('Weighted corner normals','WEIGHTED_NORMAL'); mod.keep_sharp=True; bpy.ops.object.modifier_apply(modifier=mod.name)
def cube(n,p,s,mat='CY_DullSteel',edge=.0015):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p); o=bpy.context.object; o.name=n; o.dimensions=s; bpy.ops.object.transform_apply(location=False,rotation=False,scale=True); register(o,mat)
 if edge:bevel(o,edge)
 return o
def cylinder(n,a,b,r,verts=16):
 d=Vector(b)-Vector(a); bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=d.length,location=(Vector(a)+Vector(b))/2)
 o=bpy.context.object; o.name=n; o.rotation_euler=d.to_track_quat('Z','Y').to_euler(); register(o,'CY_DullSteel')
 for p in o.data.polygons:p.use_smooth=len(p.vertices)==4
 return o
def mesh(n,v,f,smooth=True):
 me=bpy.data.meshes.new(n); me.from_pydata(v,[],f); me.update(); o=bpy.data.objects.new(n,me); bpy.context.collection.objects.link(o); register(o,'CY_DullSteel'); fix_normals(o)
 for p in me.polygons:p.use_smooth=smooth
 return o
def tube(n,points,r=.042,wall=.003,segments=24):
 pts=[Vector(p) for p in points]; v=[]; f=[]; frames=[]; u=Vector((1,0,0))
 for j,p in enumerate(pts):
  t=(pts[min(j+1,len(pts)-1)]-pts[max(j-1,0)]).normalized(); u=(u-t*u.dot(t)).normalized(); w=t.cross(u); frames.append((u.copy(),w))
 for radius in (r,r-wall):
  for p,(u,w) in zip(pts,frames):
   for k in range(segments):a=k*2*math.pi/segments; v.append(tuple(p+radius*(u*math.cos(a)+w*math.sin(a))))
 count=len(pts)*segments
 for side in (0,1):
  off=side*count
  for j in range(len(pts)-1):
   for k in range(segments):a=off+j*segments+k;b=off+j*segments+(k+1)%segments; q=(a,b,b+segments,a+segments);f.append(q if side==0 else tuple(reversed(q)))
 for end in (0,len(pts)-1):
  for k in range(segments):a=end*segments+k;b=end*segments+(k+1)%segments;f.append((a,b,b+count,a+count))
 o=mesh(n,v,f)
 for p in list(o.data.polygons)[-2*segments:]:p.use_smooth=False
 return o
def complete(name,anchors,notes):
 global parts
 material=parts[0].data.materials[0]
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name+'Mesh'
 # All parts in each export deliberately use one finish; boolean cutters can
 # otherwise leave an empty extra slot in Blender's joined mesh.
 o.data.materials.clear();o.data.materials.append(material)
 for polygon in o.data.polygons:polygon.material_index=0
 scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.1519,island_margin=.012,scale_to_bounds=True);bpy.ops.object.mode_set(mode='OBJECT');o.data.uv_layers.active.name='UVMap'
 root=bpy.data.objects.new('EP_CY_'+name,None);bpy.context.collection.objects.link(root);o.parent=root
 anchors={'ROOT':(0,0,0),'AXIS_X':(.1,0,0),'AXIS_Y':(0,.1,0),'AXIS_Z':(0,0,.1),**anchors}
 for k,p in anchors.items():
  a=bpy.data.objects.new(name+'__'+k,None);bpy.context.collection.objects.link(a);a.parent=root;a.location=p;a.empty_display_type='PLAIN_AXES';a.empty_display_size=.015
 objects=[root]+list(root.children_recursive);bpy.context.view_layer.update();bpy.ops.object.select_all(action='DESELECT')
 for q in objects:q.select_set(True)
 bpy.context.view_layer.objects.active=root;fn='EP_CY_'+name+'.fbx'
 bpy.ops.export_scene.fbx(filepath=str(OUT/fn),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False,use_mesh_modifiers=True,path_mode='RELATIVE')
 coords=[o.matrix_world@v.co for v in o.data.vertices]; o.data.calc_loop_triangles()
 assets[name]={'file':'exports/'+fn,'bounds_blender_xyz':[[min(p[i] for p in coords),max(p[i] for p in coords)] for i in range(3)],
  'mesh_objects':[{'name':o.name,'vertices':len(o.data.vertices),'triangles':len(o.data.loop_triangles),'pivot_blender_xyz':[0,0,0],'uv_layers':['UVMap']}],
  'materials':sorted({m.name for m in o.data.materials}),'anchors_blender_xyz':{name+'__'+k:list(p) for k,p in anchors.items()},'notes':notes,'sha256':hashlib.sha256((OUT/fn).read_bytes()).hexdigest()}
 asset_objects[name]=o;parts=[]

# One true open bore from inlet through the lower bend and spout.
pts=[(0,-.11,2.7),(0,-.11,.27),(0,-.11,.17)]
for i in range(1,9):
 a=math.pi*.5*i/8;pts.append((0,-.21+.1*math.cos(a),.17-.1*math.sin(a)))
pts.append((0,-.30,.07));tube('Continuous open downpipe',pts)
for z in (.78,1.84):tube('Overlapping pipe socket',[(0,-.11,z-.022),(0,-.11,z+.022)],.046,.004)
for z in (.38,1.29,2.35):
 tube('Wall pipe collar',[(0,-.11,z-.010),(0,-.11,z+.010)],.047,.005)
 cube('Wall stand-off',(0,-.038,z),(.028,.072,.035));cube('Wall anchor plate',(0,-.004,z),(.11,.008,.047))
 for x in (-.039,.039):cylinder('Anchor bolt',(x,-.011,z),(x,-.005,z),.006,6)
complete('Downpipe27',{'INLET':(0,-.11,2.7),'OUTLET':(0,-.30,.07),'WALL':(0,0,1.35)},
 '84mm OD/78mm bore. Overall inlet height2.7m above root floor. Outlet faces-Y, centre70mm above floor. Wall backing atY0; outside is-Y. Three actual standoffs. No collider/audio.')

def gutter(outlet):
 # Half-round shell, open top and ends; bottom height is localZ0.
 N=16;v=[];f=[]
 for x in (-.5,.5):
  for radius in (.070,.067):
   for i in range(N+1):a=math.pi+math.pi*i/N;v.append((x,-.11+radius*math.cos(a),.07+radius*math.sin(a)))
 stride=2*(N+1)
 for s in (0,1):
  off=s*(N+1)
  for i in range(N):f.append((off+i,off+i+1,stride+off+i+1,stride+off+i))
 for end in (0,stride):
  for i in range(N):f.append((end+i,end+i+1,end+N+2+i,end+N+1+i))
 for i in (0,N):f.append((i,stride+i,stride+N+1+i,N+1+i))
 shell=mesh('True half-round gutter shell',v,f)
 # End annuli are manufacturing cuts, not a smooth continuation of the curved
 # wall: otherwise their normals bend the whole one-metre section at each join.
 for polygon in shell.data.polygons:
  if abs(polygon.normal.x)>.95:polygon.use_smooth=False
 if outlet:
  bpy.ops.mesh.primitive_cylinder_add(vertices=32,radius=.0435,depth=.24,location=(0,-.11,0)); cutter=bpy.context.object
  mod=shell.modifiers.new('Actual downward outlet','BOOLEAN');mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cutter;bpy.context.view_layer.objects.active=shell;bpy.ops.object.modifier_apply(modifier=mod.name);bpy.data.objects.remove(cutter,do_unlink=True);fix_normals(shell)
  tube('Downpipe receiving socket',[(0,-.11,-.05),(0,-.11,.003)],.050,.0065)
 for y in (-.18,-.04):cylinder('Rolled gutter lip',(-.5,y,.07),(.5,y,.07),.0035,12)
 for x in (-.33,.33):
  cube('Gutter back bracket',(x,-.022,.038),(.019,.044,.045));cube('Gutter wall fixing',(x,-.0035,.052),(.055,.007,.055))
  cylinder('Gutter anchor bolt',(x,-.012,.052),(x,-.006,.052),.005,6)
 complete('GutterOutlet1m' if outlet else 'GutterStraight1m',{'JOIN_LEFT':(-.5,-.11,0),'JOIN_RIGHT':(.5,-.11,0),'BOTTOM':(0,-.11,0),'WALL':(0,0,.05)},
  '1m module alongX. Root at gutter bottom height; open channel above0 to.0735. Wall isY0. '+('One actual87mm opening and open downward socket: place root atZ2.7 over Downpipe27.' if outlet else 'No outlet. Ends are open for repeated segments. End caps are not included.'))
gutter(False);gutter(True)

cube('Mineral slab',(0,0,-.02),(.744,.744,.04),'CY_Mineral',.006)
complete('MineralTile0744',{'SURFACE':(0,0,0),'MODULE_RIGHT':(.75,0,0),'MODULE_BACK':(0,.75,0)},
 '.744x.744x.04m;6mm bevel; top/rootZ0, bottom-.04. Repeat at.750m for6mm joints. Original simple mineral form; no photographic/generated textures.')

# Real 1m trough with a removable-looking open grate, joined for one draw.
cube('Drain bed',(0,0,-.033),(1,.12,.006),edge=.001)
for y in (-.057,.057):cube('Drain channel side',(0,y,-.018),(1,.006,.030),edge=.001)
for y in (-.047,.047):cube('Grate longitudinal frame',(0,y,-.004),(1,.012,.008),edge=.001)
for x in (-.494,.494):cube('Drain end lip',(x,0,-.004),(.012,.09,.008),edge=.001)
for i in range(20):cube('Grate crossbar',(-.475+i*.05,0,-.004),(.014,.084,.008),edge=.001)
complete('Drain1m',{'SURFACE':(0,0,0),'JOIN_LEFT':(-.5,0,0),'JOIN_RIGHT':(.5,0,0)},
 '1m long alongX,120mm wide,36mm deep; top/rootZ0. True slots above a real channel. Recess floor rather than raising a threshold. No exported collider.')

manifest={'date':'2026-10-01','authorship':'Original project geometry authored procedurally in Blender with standard primitives and original mesh/tube construction. No external assets, images, purchases or generative service.',
 'axis_contract':'Metric X length, Z up, -Y away from wall. FBX -Z/+Y. Prior project Unity import maps(-X,Z,-Y); preserve importer basis and correct root X reflection as for other original kits. Corrected assembly maps(X,Z,-Y). Axis/root anchors are authoritative; Unity not checked here.',
 'materials':MATERIALS,'assets':assets,'total_triangles':sum(v['mesh_objects'][0]['triangles'] for v in assets.values()),
 'verification_limit':'Blender source/roundtrip/previews only. No Unity, placement, lighting or human experience claim.'}
(BASE/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
bpy.ops.wm.save_as_mainfile(filepath=str(BASE/'EchoRainwaterKit.blend'))
print('RAINWATER EXPORTED',manifest['total_triangles'],flush=True)

# Separate presentation scene: copies only, never exported.
for o in list(scene.objects):o.hide_render=True
def display(name,p):
 src=asset_objects[name];o=src.copy();o.data=src.data;scene.collection.objects.link(o);o.parent=None;o.location=p;o.hide_render=False;return o
display('Downpipe27',(0,0,0));display('GutterOutlet1m',(0,0,2.7))
for x in (-1,1):display('GutterStraight1m',(x,0,2.7))
display('Drain1m',(0,-.38,.004))
for x in (-1.125,-.375,.375,1.125):
 for y in (-1.575,-.825):display('MineralTile0744',(x,y,0))
parts=[]; wall=cube('Preview only wall',(0,.06,1.36),(3.3,.12,2.72),'CY_Mineral',.006)
wall.data.materials.clear(); wm=mats['CY_Mineral'].copy();wm.name='Preview wall only';wm.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.19,.22,.20,1);wall.data.materials.append(wm)
floor=cube('Preview only ground',(0,-.9,-.047),(4,3,.014),'CY_Mineral',0)
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.render.resolution_x=1050;scene.render.resolution_y=850;scene.render.resolution_percentage=100
scene.world.color=(.025,.025,.025);scene.view_settings.view_transform='AgX'
for n,p,e,s,c in [('Broad cool sky',(-2,-2,5),700,4,(.72,.86,1)),('Soft side',(3,-1,3.4),450,3,(.94,.9,.81))]:
 d=bpy.data.lights.new(n,'AREA');d.energy=e;d.shape='DISK';d.size=s;d.color=c;o=bpy.data.objects.new(n,d);scene.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,-.2,1.1))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();camera=bpy.context.object;scene.camera=camera
def render(name,p,target,lens=48):
 camera.location=p;camera.rotation_euler=(Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.lens=lens;scene.render.filepath=str(PRE/(name+'.png'));bpy.ops.render.render(write_still=True)
render('assembled',(-3.4,-4.8,3.0),(0,-.12,1.25),50)
render('gutter-outlet',(-.60,-.92,3.17),(0,-.10,2.69),60)
render('drain-paving',(-.86,-1.52,.95),(0,-.51,.04),50)
print('RAINWATER PREVIEWS COMPLETE',flush=True)
