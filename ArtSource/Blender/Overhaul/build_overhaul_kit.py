"""Echo Protocol Overhaul: original metric geometry, no external assets.
Blender 5.2 LTS. Source front -Y, up +Z; same -Z/+Y FBX convention as prior EP kits.
Exported roots at identity; retain importer axis transform in Unity. Front maps to Unity +Z.
"""
import bpy, math, os, json, random
from mathutils import Vector
from math import sin, cos, pi
BASE=os.path.dirname(os.path.abspath(__file__))
OUT=os.path.join(BASE,'exports'); PRE=os.path.join(BASE,'previews')
os.makedirs(OUT,exist_ok=True);os.makedirs(PRE,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
for c in list(bpy.data.collections):
 if c.name!='Collection' and c.users==0:bpy.data.collections.remove(c)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
# Base colors are intentionally not baked with shadows. Blender shader grain is preview only.
MATERIALS={
 'OV_Walnut':{'color':(.105,.049,.023,1),'rough':.34,'metal':0},
 'OV_WalnutEdge':{'color':(.16,.081,.037,1),'rough':.32,'metal':0},
 'OV_PaintedTimber':{'color':(.225,.254,.217,1),'rough':.47,'metal':0},
 'OV_Linen':{'color':(.54,.49,.37,1),'rough':.93,'metal':0},
 'OV_ChairFabric':{'color':(.18,.23,.20,1),'rough':.91,'metal':0},
 'OV_Piping':{'color':(.075,.105,.085,1),'rough':.78,'metal':0},
 'OV_DullBrass':{'color':(.32,.22,.10,1),'rough':.34,'metal':.72},
 'OV_BlackenedSteel':{'color':(.033,.039,.037,1),'rough':.47,'metal':.72},
 'OV_Porcelain':{'color':(.70,.67,.57,1),'rough':.27,'metal':0},
 'OV_Glass':{'color':(.045,.083,.087,1),'rough':.15,'metal':.1},
 'OV_Leather':{'color':(.12,.055,.026,1),'rough':.58,'metal':0},
 'OV_ClothingBlue':{'color':(.13,.19,.24,1),'rough':.94,'metal':0},
 'OV_ClothingCream':{'color':(.52,.48,.38,1),'rough':.92,'metal':0},
 'OV_Paper':{'color':(.61,.59,.47,1),'rough':.84,'metal':0},
 'OV_Rubber':{'color':(.009,.012,.011,1),'rough':.73,'metal':0}}
mats={}
for name,info in MATERIALS.items():
 m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=info['color'];p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=info['color'];p.inputs['Roughness'].default_value=info['rough'];p.inputs['Metallic'].default_value=info['metal'];mats[name]=m
 if name in ['OV_Linen','OV_ChairFabric','OV_ClothingBlue','OV_ClothingCream','OV_Leather']:
  n=m.node_tree.nodes.new('ShaderNodeTexNoise');n.inputs['Scale'].default_value=210;n.inputs['Detail'].default_value=2
  bump=m.node_tree.nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.12;bump.inputs['Distance'].default_value=.004;m.node_tree.links.new(n.outputs['Fac'],bump.inputs['Height']);m.node_tree.links.new(bump.outputs['Normal'],p.inputs['Normal'])
 if name in ['OV_Walnut','OV_WalnutEdge']:
  tc=m.node_tree.nodes.new('ShaderNodeTexCoord');mapping=m.node_tree.nodes.new('ShaderNodeVectorMath');mapping.operation='MULTIPLY';mapping.inputs[1].default_value=(3,3,.23)
  n=m.node_tree.nodes.new('ShaderNodeTexNoise');n.inputs['Scale'].default_value=9;n.inputs['Detail'].default_value=3
  ramp=m.node_tree.nodes.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].color=tuple(v*.45 for v in info['color'][:3])+(1,);ramp.color_ramp.elements[1].color=tuple(min(1,v*1.65) for v in info['color'][:3])+(1,)
  m.node_tree.links.new(tc.outputs['Generated'],mapping.inputs[0]);m.node_tree.links.new(mapping.outputs[0],n.inputs['Vector']);m.node_tree.links.new(n.outputs['Fac'],ramp.inputs[0]);m.node_tree.links.new(ramp.outputs[0],p.inputs['Base Color'])
parts=[];asset_roots=[];manifest={}
def register(o,mat):
 o.data.materials.append(mats[mat]);parts.append(o);return o
def cube(n,p,s,mat='OV_Walnut',bevel=.007,rot=(0,0,0)):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p,rotation=rot);o=bpy.context.object;o.name=n;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);register(o,mat)
 if bevel:
  mod=o.modifiers.new('Physical edge radius','BEVEL');mod.width=bevel;mod.segments=3;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
  for f in o.data.polygons:f.use_smooth=True
  mod=o.modifiers.new('Weighted surface normals','WEIGHTED_NORMAL');mod.keep_sharp=True;bpy.ops.object.modifier_apply(modifier=mod.name)
 return o
def cyl(n,p,r,h,mat='OV_DullBrass',rot=(0,0,0),verts=24):
 bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=h,location=p,rotation=rot);o=bpy.context.object;o.name=n;register(o,mat)
 for f in o.data.polygons:f.use_smooth=len(f.vertices)==4
 return o
def rod(n,a,b,r,mat='OV_DullBrass'):
 d=Vector(b)-Vector(a);o=cyl(n,(Vector(a)+Vector(b))/2,r,d.length,mat);o.rotation_euler=d.to_track_quat('Z','Y').to_euler();return o
def path(n,pts,r,mat='OV_DullBrass',closed=False,res=2):
 cu=bpy.data.curves.new(n,'CURVE');cu.dimensions='3D';cu.resolution_u=12;cu.bevel_depth=r;cu.bevel_resolution=res
 sp=cu.splines.new('POLY');sp.points.add(len(pts)-1)
 for point,p in zip(sp.points,pts):point.co=(*p,1)
 sp.use_cyclic_u=closed;o=bpy.data.objects.new(n,cu);bpy.context.collection.objects.link(o);bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.convert(target='MESH');o=bpy.context.object;register(o,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o
def torus(n,p,r,t,mat='OV_DullBrass',rot=(0,0,0)):
 bpy.ops.mesh.primitive_torus_add(major_radius=r,minor_radius=t,major_segments=32,minor_segments=8,location=p,rotation=rot);o=bpy.context.object;o.name=n;register(o,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o
def lathe(n,profile,p,mat,segments=32):
 v=[];f=[]
 for r,z in profile:
  for i in range(segments):a=i*2*pi/segments;v.append((p[0]+r*cos(a),p[1]+r*sin(a),p[2]+z))
 for j in range(len(profile)-1):
  for i in range(segments):k=j*segments+i;k2=j*segments+(i+1)%segments;f.append((k,k2,k2+segments,k+segments))
 f += [tuple(reversed(range(segments))),tuple((len(profile)-1)*segments+i for i in range(segments))]
 return mesh(n,v,f,mat)
def mesh(n,v,f,mat):
 me=bpy.data.meshes.new(n);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(n,me);bpy.context.collection.objects.link(o);register(o,mat)
 for face in me.polygons:face.use_smooth=True
 # Stable normalized UVs for explicit source mesh, adequate for tiling fabric later.
 uv=me.uv_layers.new(name='UVMap');bounds=[(min(x[k] for x in v),max(x[k] for x in v)) for k in range(3)]
 axes=sorted(range(3),key=lambda k:bounds[k][1]-bounds[k][0],reverse=True)[:2]
 for poly in me.polygons:
  for li in poly.loop_indices:
   co=me.vertices[me.loops[li].vertex_index].co;uv.data[li].uv=tuple((co[k]-bounds[k][0])/max(.001,bounds[k][1]-bounds[k][0]) for k in axes)
 return o
def cushion(n,p,s,mat='OV_ChairFabric',tilt=0,tufts=False):
 # Superellipsoid is a single continuous upholstered surface with rounded corners.
 verts=[];faces=[];nx=48;ny=24;exp=.34
 power=lambda x,e:math.copysign(abs(x)**e,x)
 for j in range(ny+1):
  a=-pi/2+j*pi/ny
  for i in range(nx):
   b=i*2*pi/nx;x=s[0]*.5*power(cos(a),exp)*power(cos(b),exp);y=s[1]*.5*power(cos(a),exp)*power(sin(b),exp);z=s[2]*.5*power(sin(a),exp)
   # Very small authored irregularity, not inflated blanket noise.
   d=(.0018*sin(i*.81+j*1.13)+.0011*sin(i*1.63-j*.62))*abs(cos(a));y+=d
   if tufts and y<-.01:
    for tx in [-.17,.17]:
     for tz in [-.16,.15]:y+=.012*math.exp(-((x-tx)**2+(z-tz)**2)/.0018)
   yy=y*cos(tilt)-z*sin(tilt);zz=y*sin(tilt)+z*cos(tilt);verts.append((p[0]+x,p[1]+yy,p[2]+zz))
 for j in range(ny):
  for i in range(nx):a=j*nx+i;b=j*nx+(i+1)%nx;faces.append((a,b,b+nx,a+nx))
 return mesh(n,verts,faces,mat)
def rounded_rect(cx,cy,z,w,d,rad=.06,vertical=False):
 out=[]
 for sx,sy,start in [(1,1,0),(-1,1,90),(-1,-1,180),(1,-1,270)]:
  for i in range(7):a=math.radians(start+i*15);x=cx+sx*(w/2-rad)+rad*cos(a);y=cy+sy*(d/2-rad)+rad*sin(a);out.append((x,y,z) if not vertical else (x,z,y))
 return out

def join_part(name,items,pivot=(0,0,0)):
 bpy.ops.object.select_all(action='DESELECT')
 for o in items:o.select_set(True)
 bpy.context.view_layer.objects.active=items[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
 scene.cursor.location=pivot;bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return o

def export(name,notes,groups=None):
 global parts
 root=bpy.data.objects.new('EP_OV_'+name,None);bpy.context.collection.objects.link(root)
 if groups:
  used=[]
  for gn,gitems,pivot in groups:
   o=join_part(gn,gitems,pivot);o.parent=root;used+=gitems
 else:
  o=join_part(name+'_Geometry',parts);o.parent=root
 objects=[root]+list(root.children_recursive)
 collection=bpy.data.collections.new('Asset / '+name);scene.collection.children.link(collection)
 for obj in objects:
  for old in list(obj.users_collection):old.objects.unlink(obj)
  collection.objects.link(obj)
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=root
 bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'EP_OV_'+name+'.fbx'),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False,use_mesh_modifiers=True)
 all_coords=[o.matrix_world @ Vector(c) for o in objects if o.type=='MESH' for c in o.bound_box];bounds=[[min(v[i] for v in all_coords),max(v[i] for v in all_coords)] for i in range(3)]
 manifest[name]={'file':'exports/EP_OV_'+name+'.fbx','source_front':'-Y','unity_front':'+Z with importer axis transform retained','bounds_blender_xyz':bounds,'dimensions_width_depth_height':[b-a for a,b in bounds],'materials':sorted({m.name for o in objects if o.type=='MESH' for m in o.data.materials if m}),'mesh_objects':[{ 'name':o.name,'pivot_blender_xyz':list(o.location),'vertices':len(o.data.vertices),'triangles':sum(len(f.vertices)-2 for f in o.data.polygons)} for o in objects if o.type=='MESH'],'notes':notes}
 for o in objects:o.hide_render=True;o.hide_set(True)
 asset_roots.append(root);parts=[];print('EXPORTED',name,manifest[name]['dimensions_width_depth_height'],flush=True)

# 1. Reception. A genuine L counter: customer face -Y, staff work side +Y.
for x in [-1.10,1.10]:cube('Structural end',(x,0,.51),(.065,.64,1.02),'OV_Walnut')
cube('Raised plinth',(0,.005,.065),(2.25,.62,.13),'OV_Walnut')
cube('Front panel backing',(0,-.294,.55),(2.23,.045,.92),'OV_PaintedTimber')
for x in [-1.11,-.555,0,.555,1.11]:cube('Front muntin',(x,-.328,.55),(.044,.029,.93),'OV_WalnutEdge',.004)
for z in [.14,.97]:cube('Front rail',(0,-.33,z),(2.27,.045,.055),'OV_WalnutEdge',.005)
for x in [-.831,-.277,.277,.831]:
 cube('Raised face panel',(x,-.324,.553),(.489,.026,.722),'OV_PaintedTimber',.014)
 # Narrow inset bead reads from the approach without introducing patterned noise.
 for xx in [x-.238,x+.238]:cube('Panel beading',(xx,-.344,.55),(.010,.012,.70),'OV_WalnutEdge',.002)
cube('Counter top',(0,0,1.041),(2.4,.73,.058),'OV_WalnutEdge',.02)
cube('Writing ledge',(-.19,-.47,.928),(1.78,.29,.038),'OV_Walnut',.014)
for x in [-.92,.57]:cube('Ledge bracket',(x,-.425,.854),(.033,.19,.10),'OV_BlackenedSteel',.005,rot=(.22,0,0))
# Short return at the worker's right; clear center opening on staff side.
cube('Return panel',(1.07,.73,.51),(.065,1.00,1.02),'OV_PaintedTimber')
cube('Return top',(.90,.833,1.041),(.43,.935,.058),'OV_WalnutEdge',.018)
cube('Staff shelf',(.39,.03,.60),(1.13,.49,.027),'OV_Walnut')
cube('Drawer box',(.60,-.03,.86),(.76,.46,.19),'OV_Walnut')
cube('Drawer face',(.60,.214,.855),(.78,.033,.185),'OV_WalnutEdge')
rod('Drawer handle',(.48,.256,.855),(.72,.256,.855),.010)
for x in [.48,.72]:rod('Handle standoff',(x,.225,.855),(x,.256,.855),.009)
# A cable grommet, protective brass rail and a small document tray.
cyl('Desk grommet',(.97,.10,1.073),.028,.005,'OV_Rubber')
rod('Brass counter guard',(-1.02,-.255,1.085),(.98,-.255,1.085),.007)
cube('Document tray base',(-.77,.015,1.085),(.36,.27,.018),'OV_BlackenedSteel')
for x in [-.95,-.59]:cube('Tray edge',(x,.015,1.11),(.008,.27,.053),'OV_BlackenedSteel',.002)
cube('Tray back',(-.77,.145,1.11),(.36,.009,.053),'OV_BlackenedSteel',.002)
for i in range(3):cube('Form',(-.77,.016,1.10+i*.002),(.30,.215,.001),'OV_Paper',0,rot=(0,0,.015*i))
export('ReceptionCounter','Static L counter; customer side +UnityZ. Root at floor center of front. Staff side open. Simple box colliders for front/return only. Letter placement top height 1.07 m or writing ledge 0.95 m.')

# Separate wall-mounted key / correspondence cabinet, not a mandatory narrative prop.
cube('Key cabinet back',(0,.042,.45),(1.14,.06,.9),'OV_Walnut')
for x in [-.575,.575]:cube('Cabinet side',(x,-.045,.45),(.055,.23,.85),'OV_WalnutEdge')
for z in [0,.90]:cube('Cabinet top bottom',(0,-.045,z),(1.2,.23,.05),'OV_WalnutEdge')
for z in [.26,.50,.73]:cube('Mail shelf',(-.20,-.053,z),(.73,.21,.025),'OV_WalnutEdge',.004)
for x in [-.37,0,.195]:cube('Mail divider',(x,-.05,.48),(.018,.20,.87),'OV_Walnut',.003)
for i,z in enumerate([.034,.28,.52,.75]):
 for j,x in enumerate([-.47,-.18,.1]):
  if (i+j)%3!=0:cube('Folded envelope',(x,-.068,z),(.11,.12,.004),'OV_Paper',.001,rot=(0,.10,.05))
for z in [.23,.52,.80]:
 for x in [.31,.46]:
  cyl('Hook back',(x,-.015,z),.016,.008,'OV_DullBrass',(pi/2,0,0),16)
  path('Hook',[(x,-.02,z),(x,-.09,z),(x,-.102,z+.012)],.0045)
# One hanging key: scale discernible only close, no gigantic decorative key.
torus('Key ring',(.31,-.084,.48),.019,.0025,'OV_DullBrass',(pi/2,0,0));rod('Key stem',(.31,-.084,.45),(.31,-.084,.406),.0035)
cube('Key bit',(.317,-.084,.409),(.018,.006,.009),'OV_DullBrass',.001)
export('KeyMailCabinet','Wall mounted. Root at lower midpoint. Back at source Y +0.07 / Unity -Z; front +Z. No collision needed unless reachable.')

# 2. Window: frame/glass and both drapes are separate mesh groups for material/raycast use.
# Window bottom at 0.75 m, clear width 1.46 m; complete assembly includes radiator below.
for x in [-.77,.77]:cube('Window casing',(x,0,1.62),(.105,.18,1.65),'OV_PaintedTimber',.008)
for z in [.75,2.49]:cube('Window casing',(0,0,z),(1.64,.18,.09),'OV_PaintedTimber',.007)
for x in [-.70,0,.70]:cube('Sash upright',(x,-.025,1.62),(.042,.075,1.65),'OV_PaintedTimber',.004)
for z in [.82,1.58,2.41]:cube('Sash rail',(0,-.035,z),(1.46,.07,.043),'OV_PaintedTimber',.004)
cube('Window sill',(0,-.075,.746),(1.84,.34,.064),'OV_WalnutEdge',.012)
for x in [-.36,.36]:
 for z,h in [(1.20,.70),(1.985,.75)]:cube('Window glazing',(x,.015,z),(.655,.012,h),'OV_Glass',.001)
rod('Curtain pole',(-1.04,-.36,2.55),(1.04,-.36,2.55),.014)
for x in [-1.075,1.075]:
 o=lathe('Curtain pole finial',[(.001,-.02),(.023,-.012),(.033,0),(.023,.024),(.001,.03)],(0,0,0),'OV_WalnutEdge',24);o.rotation_euler=(0,pi/2,0);o.location=(x,-.36,2.55)
for x in [-.90,.90]:rod('Curtain bracket',(x,.02,2.50),(x,-.36,2.50),.009);rod('Curtain bracket',(x,-.36,2.50),(x,-.36,2.55),.009)
frame=parts.copy();parts=[]
curtains=[]
for side in [-1,1]:
 v=[];f=[];nu=40;nv=40
 for j in range(nv+1):
  t=j/nv;z=2.51-2.14*t
  # Fabric gathered mid-way, then released: silhouette is not a straight corrugated sheet.
  gather=math.exp(-((t-.57)/.13)**2);width=.51-.25*gather;centre=side*(.72+.105*gather)
  for i in range(nu+1):
   u=i/nu;x=centre+(u-.5)*width;fold=sin(u*pi*10+.10*sin(t*6))* (.045-.021*gather);y=-.33+fold-.032*sin(t*pi)+.009*sin(t*15+u*9)
   hem=.018*sin(u*8*pi)+.011*sin(u*3*pi);v.append((x,y,z+hem*t**9))
 for j in range(nv):
  for i in range(nu):a=j*(nu+1)+i;f.append((a,a+1,a+nu+2,a+nu+1))
 o=mesh(('Left' if side<0 else 'Right')+' gathered curtain',v,f,'OV_Linen');mod=o.modifiers.new('Linen thickness','SOLIDIFY');mod.thickness=.0016;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
 # A real weighted hem and restrained tieback.
 path('Curtain weighted hem',[v[nv*(nu+1)+i] for i in range(nu+1)],.003,'OV_Linen')
 for i in range(8):torus('Curtain ring',(side*.72+(i/7-.5)*.49,-.36,2.53),.021,.003,'OV_DullBrass',(pi/2,0,0))
 x=side*.825
 path('Fabric tieback',[(x-.13,-.40,1.29),(x,-.43,1.25),(x+.13,-.40,1.29)],.012,'OV_Piping')
 curtains+=parts;parts=[]
# Repeated cast radiator sections with curved top and bottom, valves connect to floor.
for i in range(12):
 x=-.58+i*.105
 path('Cast radiator section',[(x,-.24,.23),(x,-.24,.62),(x,-.21,.66),(x,-.09,.66),(x,-.06,.62),(x,-.06,.23),(x,-.09,.19),(x,-.21,.19),(x,-.24,.23)],.026,'OV_Porcelain',True,3)
 rod('Section cross brace',(x,-.24,.33),(x,-.06,.33),.025,'OV_Porcelain')
for z in [.25,.59]:rod('Radiator header',(-.65,-.15,z),(.65,-.15,z),.028,'OV_Porcelain')
for x in [-.70,.70]:rod('Radiator feed',(x,-.15,.04),(x,-.15,.25),.020,'OV_BlackenedSteel');rod('Radiator feed',(x,-.15,.25),(x-(.05 if x>0 else -.05),-.15,.25),.020,'OV_BlackenedSteel')
cyl('Valve',(.70,-.15,.35),.05,.033,'OV_Porcelain');rod('Valve stem',(.70,-.15,.25),(.70,-.15,.34),.012,'OV_DullBrass')
radiator=parts.copy();parts=frame+curtains+radiator
export('DressedWindow','Floor-origin window assembly, sill 0.75 m, pole 2.55 m. Wall front source Y=+0.09 behind window. Keep glass opaque blue-black until integrator provides exterior; no false transparency into void. Curtain geometry is double-sided solid fabric. Radiator separate mesh.', [('WindowFrame',frame,(0,0,0)),('Curtains',curtains,(0,0,0)),('Radiator',radiator,(0,0,0))])

# 3. Door: frame separate from leaf. Left hinge in front elevation. Empty-free mesh pivot exported explicitly.
for side in [-1,1]:
 x=side*.495;cube('Structural jamb',(x,0,1.055),(.07,.18,2.11),'OV_PaintedTimber',.004)
 cube('Front architrave',(side*.533,-.123,1.067),(.085,.050,2.134),'OV_PaintedTimber',.007)
 cube('Architrave bead',(side*.565,-.151,1.067),(.014,.012,2.134),'OV_WalnutEdge',.003)
 cube('Rear architrave',(side*.533,.123,1.067),(.085,.035,2.134),'OV_PaintedTimber',.004)
 cube('Door stop',(side*.466,.037,1.055),(.015,.038,2.11),'OV_PaintedTimber',.002)
cube('Jamb head',(0,0,2.145),(1.06,.18,.075),'OV_PaintedTimber',.004)
cube('Front head trim',(0,-.123,2.18),(1.16,.050,.092),'OV_PaintedTimber',.006)
cube('Rear head trim',(0,.123,2.18),(1.16,.035,.092),'OV_PaintedTimber',.004)
cube('Threshold',(0,0,.012),(.93,.24,.024),'OV_WalnutEdge',.004)
frame=parts.copy();parts=[]
leafWidth=.895;leafBottom=.025;leafTop=2.092;hinge=(-.4475,0,0)
# Full thin core ensures no gaps; raised timber stiles and rails provide true shallow recesses.
cube('Leaf core',(0,0,(leafTop+leafBottom)/2),(leafWidth,.033,leafTop-leafBottom),'OV_PaintedTimber',.004)
for x in [-.385,.385]:cube('Leaf stile',(x,-.012,1.058),(.12,.046,2.065),'OV_PaintedTimber',.004)
for z,h in [(.087,.12),(.74,.12),(1.98,.22)]:cube('Leaf rail',(0,-.012,z),(.65,.046,h),'OV_PaintedTimber',.004)
for z,h in [(.408,.50),(1.342,1.04)]:
 cube('Recessed timber panel',(0,-.020,z),(.617,.026,h),'OV_PaintedTimber',.007)
 for x in [-.322,.322]:cube('Panel bevel bead',(x,-.039,z),(.018,.013,h+.025),'OV_WalnutEdge',.003)
 for zz in [z-h/2-.009,z+h/2+.009]:cube('Panel bevel bead',(0,-.039,zz),(.661,.013,.018),'OV_WalnutEdge',.003)
# Hinges and two handles move with leaf. Pivot center remains at lower hinge axis.
for z in [.28,1.12,1.91]:
 cyl('Hinge barrel',(-.452,.003,z),.011,.092,'OV_DullBrass');cube('Hinge leaf',(-.428,.008,z),(.05,.005,.073),'OV_DullBrass',.001)
for sy in [-1,1]:
 cyl('Handle rose',(.328,sy*.031,1.012),.035,.008,'OV_DullBrass',(pi/2,0,0))
 rod('Spindle',(.328,sy*.035,1.012),(.328,sy*.093,1.012),.012)
 path('Lever',[(.328,sy*.09,1.012),(.301,sy*.113,1.012),(.225,sy*.113,1.005)],.011)
 cyl('Keyhole rose',(.328,sy*.034,.85),.022,.006,'OV_DullBrass',(pi/2,0,0));cube('Key slot',(.328,sy*.04,.85),(.006,.003,.021),'OV_Rubber',.001)
leaf=parts.copy();parts=frame+leaf
export('DomesticDoor','Clear frame opening 0.920 m × 2.107 m (before threshold). Leaf width 0.895 m. DoorLeafPivot mesh at Blender (-0.4475,0,0), maps to Unity (-0.4475,0,0); rotate local vertical axis through importer-correct transform. Leaf includes both handles/hinges; frame static. No colliders authored.', [('DoorFrame',frame,(0,0,0)),('DoorLeafPivot',leaf,hinge)])

# 4. Armchair: wooden support, shaped cushions, piping and draped linen.
# Feet are individually tapered lathed forms, not cylinders through the upholstered arms.
for x in [-.32,.32]:
 for y in [-.31,.31]:lathe('Turned walnut foot',[(.029,0),(.033,.04),(.025,.13),(.040,.19)],(x,y,.025),'OV_WalnutEdge',20)
cube('Chair base',(0,.005,.275),(.77,.76,.16),'OV_Walnut',.028)
cushion('Seat cushion',(0,-.06,.475),(.60,.65,.185))
# Back angle inclines away from occupant, supplying a recognizable human resting pose.
cushion('Outer upholstered back',(0,.315,.765),(.79,.21,.68),'OV_ChairFabric',tilt=-.13)
cushion('Inset back cushion',(0,.206,.77),(.61,.145,.53),'OV_ChairFabric',tilt=-.13,tufts=True)
for side in [-1,1]:
 cushion('Upholstered arm',(side*.377,-.02,.619),(.185,.82,.228))
 cushion('Side skirt',(side*.383,.035,.415),(.105,.72,.26),'OV_ChairFabric')
 # Carved front post emerges under the arm without looking like exposed prototype frame.
 lathe('Front arm support',[(.03,0),(.045,.055),(.029,.13),(.045,.21),(.038,.27)],(side*.37,-.33,.27),'OV_WalnutEdge',24)
path('Seat welt',rounded_rect(0,-.06,.515,.604,.658,.083),.0035,'OV_Piping',True)
path('Lower upholstery welt',rounded_rect(0,.005,.305,.75,.735,.07),.003,'OV_Piping',True)
# Front-facing piping around the back follows a rounded rectangle in XZ.
welt=[]
for x,unused,z in rounded_rect(0,0,0,.55,.45,.063,True):
 local_y=-.0725*max(.001,1-(abs(x)/.305)**(2/.34)-(abs(z)/.265)**(2/.34))**(.34/2)-.004
 welt.append((x,.206+local_y*cos(-.13)-z*sin(-.13),.77+local_y*sin(-.13)+z*cos(-.13)))
path('Back cushion welt',welt,.0034,'OV_Piping',True)
for x in [-.17,.17]:
 for z in [.62,.93]:cyl('Upholstery button',(x,.125+(z-.77)*.13,z),.010,.008,'OV_Piping',(pi/2,0,0),16)
# A small throw draped over one arm, shape authored over arm then falling outside.
v=[];f=[];nu=24;nv=34
for j in range(nv+1):
 t=j/nv;x=.22+.39*t;z=.753 if t<.48 else .753-(t-.48)*.58;x=.22+.40*min(t,.6)+.04*max(0,t-.6)
 for i in range(nu+1):u=i/nu;y=-.30+u*.45;fold=.011*sin(u*8*pi+.3*sin(t*7));v.append((x,y,z+fold+.01*sin(t*6)))
for j in range(nv):
 for i in range(nu):a=j*(nu+1)+i;f.append((a,a+1,a+nu+2,a+nu+1))
o=mesh('Linen throw over arm',v,f,'OV_Linen');mod=o.modifiers.new('Woven cloth thickness','SOLIDIFY');mod.thickness=.002;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
for i in range(17):y=-.30+i*.45/16;path('Throw fringe',[(.468,y,.46),(.47+.004*sin(i),y+.002,.43+.007*sin(i))],.0014,'OV_Linen',res=1)
export('Armchair','Adult-scale armchair, front +UnityZ. Seat top approx 0.565 m, total height approx 1.1 m. Cloth folds are modeled geometry; shader microtexture is preview-only. Simplified collider recommended: one box on base/back, not every curve.')

# 5. Open leather suitcase with separated lid pivot and removable contents.
# Back hinge at +Y, front buckles -Y. Base floor footprint 0.76 x 0.48 m.
cube('Suitcase bottom',(0,0,.055),(.76,.48,.065),'OV_Leather',.025)
for x in [-.368,.368]:cube('Suitcase side',(x,0,.16),(.026,.465,.22),'OV_Leather',.014)
for y in [-.228,.228]:cube('Suitcase side',(0,y,.16),(.73,.026,.22),'OV_Leather',.012)
cube('Lining base',(0,0,.093),(.695,.410,.013),'OV_Linen',.013)
path('Case piping',rounded_rect(0,0,.253,.75,.475,.035),.004,'OV_WalnutEdge',True)
for x in [-.24,.24]:
 cube('Buckle mounting',(x,-.249,.15),(.055,.009,.05),'OV_DullBrass',.004);cube('Latch',(x,-.260,.16),(.032,.016,.031),'OV_DullBrass',.004)
path('Suitcase carry handle',[(-.095,-.252,.17),(-.075,-.307,.17),(.075,-.307,.17),(.095,-.252,.17)],.012,'OV_Leather',False,3)
for x in [-.087,.087]:cyl('Handle rivet',(x,-.250,.17),.011,.006,'OV_DullBrass',(pi/2,0,0),16)
base=parts.copy();parts=[]
# Lid at rest in open position (~100deg from closed plane), preserving hinge parent pivot.
hinge=(0,.235,.252)
cube('Lid shell',(0,.237,.505),(.75,.055,.51),'OV_Leather',.026)
cube('Lid lining',(0,.203,.50),(.693,.015,.453),'OV_Linen',.02)
path('Lid piping',rounded_rect(0,.505,.193,.738,.497,.04,True),.004,'OV_WalnutEdge',True)
for x in [-.19,.19]:cube('Elastic retaining band',(x,.19,.50),(.026,.009,.42),'OV_Piping',.003,rot=(0,0,.015 if x<0 else -.015))
# Cloth pocket is a subtly bulged surface, not a black label.
cushion('Lid fabric pocket',(0,.17,.39),(.42,.046,.15),'OV_ClothingCream')
for x in [-.26,.26]:cyl('Case hinge',(x,.235,.252),.010,.085,'OV_DullBrass',(0,pi/2,0),16)
lid=parts.copy();parts=[]
cushion('Folded blue shirt',(-.15,-.025,.16),(.36,.35,.11),'OV_ClothingBlue')
cushion('Folded cream sweater',(.17,.015,.178),(.30,.37,.145),'OV_ClothingCream')
# Shirt collar, placket and buttons clarify clothes rather than generic cushions.
mesh('Shirt collar left',[(-.23,.065,.216),(-.18,.11,.219),(-.15,.058,.221),(-.17,.015,.219)],[(0,1,2,3)],'OV_ClothingBlue')
mesh('Shirt collar right',[(-.15,.058,.222),(-.12,.11,.220),(-.07,.065,.217),(-.13,.015,.220)],[(0,1,2,3)],'OV_ClothingBlue')
cube('Folded shirt placket',(-.15,-.057,.219),(.012,.145,.003),'OV_ClothingBlue',.001)
for yy in [-.11,-.065,-.02]:cyl('Shirt button',(-.15,yy,.222),.004,.002,'OV_Porcelain',verts=12)
for i in range(13):path('Sweater hem rib',[(.05+i*.018,-.15,.242),(.05+i*.018,-.116,.249)],.0015,'OV_Linen',res=1)
# Soft scarf spilling over the near rim makes the open suitcase readable from eye height.
v=[];f=[];nu=18;nv=28
for j in range(nv+1):
 t=j/nv;y=.05-.41*t;z=.285-.235*max(0,(t-.76)/.24)
 for i in range(nu+1):u=i/nu;x=.22+(u-.5)*.18;v.append((x+.013*sin(t*5+u*3),y,z+.009*sin(u*6*pi+t*2)))
for j in range(nv):
 for i in range(nu):a=j*(nu+1)+i;f.append((a,a+1,a+nu+2,a+nu+1))
o=mesh('Scarf over case rim',v,f,'OV_Linen');mod=o.modifiers.new('Woven scarf thickness','SOLIDIFY');mod.thickness=.0015;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name);clothes=parts.copy();parts=base+lid+clothes
export('OpenSuitcase','Floor root. SuitcaseLidPivot is at Blender (0,+0.235,+0.252), maps Unity (0,+0.252,-0.235). Lid modeled open upright; rotate only if needed, current pose is open. Clothes separate removable mesh; no narrative interaction built in.', [('SuitcaseBase',base,(0,0,0)),('SuitcaseLidPivot',lid,hinge),('SuitcaseClothing',clothes,(0,0,0))])

# 6. Hall/bedroom table: shallow console with tray and key. Key isolated for interaction.
cube('Console top',(0,0,.765),(.79,.37,.042),'OV_WalnutEdge',.013)
cube('Console apron',(0,.012,.658),(.68,.285,.17),'OV_Walnut',.008)
cube('Console drawer',(0,-.145,.669),(.58,.025,.115),'OV_WalnutEdge',.006)
for x in [-.27,.27]:
 for y in [-.11,.11]:
  lathe('Tapered console foot',[(.020,0),(.024,.035),(.016,.13),(.024,.61)],(x,y,.02),'OV_Walnut',20)
rod('Console drawer pull',(-.066,-.188,.668),(.066,-.188,.668),.009)
for x in [-.066,.066]:rod('Pull mount',(x,-.163,.668),(x,-.188,.668),.008)
cube('Lower shelf',(0,.006,.22),(.605,.252,.019),'OV_Walnut',.006)
# Dish with lathed concave rim; key lies visibly on it. Small folded receipt provides context.
lathe('Ceramic catchall',[(0,0),(.097,0),(.108,.007),(.110,.021),(.104,.030),(.093,.029),(.088,.014),(0,.012)],(-.17,-.027,.79),'OV_Porcelain',40)
cube('Folded receipt',(.15,-.016,.79),(.14,.20,.0015),'OV_Paper',0,rot=(0,0,.17))
static=parts.copy();parts=[]
torus('Key ring',(-.185,-.025,.826),.020,.0025,'OV_DullBrass');rod('Key shaft',(-.17,-.035,.826),(-.115,-.071,.826),.0035);cube('Key tooth',(-.115,-.067,.826),(.012,.018,.006),'OV_DullBrass',.001)
key=parts.copy();parts=static+key
export('KeyConsole','Shallow 0.79 × 0.37 m table, top 0.786 m. Key mesh isolated for optional interaction/removal; root pivot of Key is at key ring centre. Tray/receipt static.', [('ConsoleAndTray',static,(0,0,0)),('Key',key,(-.185,-.025,.826))])

# Source archive includes each export group at origin, hidden for isolation; collections remain editable.
with open(os.path.join(BASE,'manifest.json'),'w') as f:json.dump({'date':'2026-10-01','authorship':'Original project geometry; no external assets or generative voices','axis_contract':'Blender X width, -Y front, Z up. FBX -Z forward / +Y up, source importer axis correction must be retained. Nominal front Unity +Z.','materials':MATERIALS,'assets':manifest},f,indent=2)
for obj in [asset_roots[0]]+list(asset_roots[0].children_recursive):obj.hide_set(False);obj.hide_render=False
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'EchoOverhaulKit.blend'))
for obj in [asset_roots[0]]+list(asset_roots[0].children_recursive):obj.hide_set(True);obj.hide_render=True
# Neutral physically lit turntable views for integration, not claims of Game-view quality.
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.render.resolution_x=1200;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.world.color=(.1,.1,.1);scene.view_settings.view_transform='AgX'
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.006));ground=bpy.context.object;ground.name='Preview ground';gm=bpy.data.materials.new('Preview only matte');gm.diffuse_color=(.085,.093,.086,1);ground.data.materials.append(gm)
for name,p,power,size,color in [('Key',(-3,-4,5),550,4,(1,.85,.68)),('Fill',(4,-2,3),300,3,(.64,.78,1)),('Rim',(0,3,4),450,3,(1,.90,.75))]:
 data=bpy.data.lights.new(name,'AREA');data.energy=power;data.shape='DISK';data.size=size;data.color=color;o=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,0,.8))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();camera=bpy.context.object;scene.camera=camera;camera.data.lens=48
for root in asset_roots:
 name=root.name.replace('EP_OV_','')
 if os.environ.get('EP_PREVIEW_ONLY') and name not in os.environ['EP_PREVIEW_ONLY'].split(','):continue
 entry=manifest[name];w,d,h=entry['dimensions_width_depth_height'];centre=Vector((0,0,h*.47));distance=max(w,h,d)*2.3
 camera.location=centre+Vector((-.60,-1,.57)).normalized()*distance;camera.rotation_euler=(centre-camera.location).to_track_quat('-Z','Y').to_euler()
 for o in [root]+list(root.children_recursive):o.hide_render=False;o.hide_set(False)
 scene.render.filepath=os.path.join(PRE,name+'.png');bpy.ops.render.render(write_still=True)
 for o in [root]+list(root.children_recursive):o.hide_render=True;o.hide_set(True)
print('OVERHAUL KIT COMPLETE',OUT,flush=True)
