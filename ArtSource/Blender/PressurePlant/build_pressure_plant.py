"""Original Echo Protocol pressure plant. Blender 5.2, metres, no external assets.
Only writes this source directory. Source front -Y/up Z; FBX -Z/+Y as prior kits.
Closed cabinet preserves the 1.7 x 6 x 2.7 m baffle envelope; no gameplay exported.
"""
import bpy, math, os, json, random
import numpy as np
from mathutils import Vector
from math import sin, cos, pi
BASE=os.path.dirname(os.path.abspath(__file__))
OUT=os.path.join(BASE,'exports');PRE=os.path.join(BASE,'previews');TEX=os.path.join(BASE,'textures')
for p in [OUT,PRE,TEX]:os.makedirs(p,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
MATERIALS={
 'PP_ServiceEnamel':{'color':(.285,.305,.271,1),'roughness':.57,'metallic':.12},
 'PP_ReplacementPanel':{'color':(.31,.321,.291,1),'roughness':.63,'metallic':.10},
 'PP_ReceiverPaint':{'color':(.15,.209,.219,1),'roughness':.54,'metallic':.13},
 'PP_MotorPaint':{'color':(.095,.137,.143,1),'roughness':.51,'metallic':.15},
 'PP_PipePaint':{'color':(.048,.061,.056,1),'roughness':.56,'metallic':.24},
 'PP_CastIron':{'color':(.078,.094,.086,1),'roughness':.60,'metallic':.7},
 'PP_MachinedSteel':{'color':(.25,.28,.26,1),'roughness':.39,'metallic':.8},
 'PP_FastenerZinc':{'color':(.39,.405,.37,1),'roughness':.38,'metallic':.82},
 'PP_Rubber':{'color':(.016,.021,.018,1),'roughness':.87,'metallic':0},
 'PP_Recess':{'color':(.021,.027,.024,1),'roughness':.89,'metallic':0},
 'PP_Oxidation':{'color':(.17,.079,.034,1),'roughness':.87,'metallic':.05},
 'PP_PlateIvory':{'color':(.49,.477,.373,1),'roughness':.67,'metallic':.05}}
mats={}
for name,data in MATERIALS.items():
 m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=data['color'];p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=data['color'];p.inputs['Roughness'].default_value=data['roughness'];p.inputs['Metallic'].default_value=data['metallic'];mats[name]=m
# Optional tileable maps: restrained authored paint variation, not a photograph.
# Geometry remains useful with the numerical material values if these are omitted.
rng=np.random.default_rng(291);N=512;field=np.zeros((N,N),np.float32)
for scale,weight in [(2,.42),(8,.28),(32,.16),(128,.09),(512,.05)]:
 small=rng.random((scale,scale)).astype(np.float32)-.5
 tile=np.repeat(np.repeat(small,N//scale,0),N//scale,1)
 for step in [max(1,N//scale//2),max(1,N//scale//4),1]:tile=(tile+np.roll(tile,step,0)+np.roll(tile,-step,0)+np.roll(tile,step,1)+np.roll(tile,-step,1))/5
 field+=tile*weight
field/=max(.01,float(np.std(field)));field=np.clip(field,-2.5,2.5)
for suffix,base in [('BaseColor',np.array(MATERIALS['PP_ServiceEnamel']['color'][:3])),('Roughness',np.array([.57,.57,.57]))]:
 rgb=np.clip(base[None,None,:]+field[:,:,None]*(.009 if suffix=='BaseColor' else .024),0,1)
 rgba=np.concatenate((rgb,np.ones((N,N,1))),2).astype(np.float32)
 im=bpy.data.images.new('PP_Enamel_'+suffix,width=N,height=N,alpha=True);im.pixels.foreach_set(rgba.ravel());im.filepath_raw=os.path.join(TEX,'PP_Enamel_'+suffix+'.png');im.file_format='PNG';im.save()
 im.colorspace_settings.name='Non-Color' if suffix=='Roughness' else 'sRGB'
 node=mats['PP_ServiceEnamel'].node_tree.nodes.new('ShaderNodeTexImage');node.image=im
 # Node intentionally unconnected: neutral FBX numeric material is authoritative.
 # Root can add these optional maps in URP; avoids undocumented exporter shader conversion.
MATERIALS['PP_ServiceEnamel']['optional_maps']={'base_color':'textures/PP_Enamel_BaseColor.png','roughness':'textures/PP_Enamel_Roughness.png','resolution':512,'note':'Original tileable paint maps. Roughness is linear; invert for Unity smoothness. Optional, not connected to exported FBX shader.'}

parts=[];roots=[];manifest={}
def register(o,mat):
 o.data.materials.append(mats[mat]);parts.append(o);return o
def normals(o):
 for f in o.data.polygons:f.use_smooth=True
 mod=o.modifiers.new('Weighted hard surface normals','WEIGHTED_NORMAL');mod.keep_sharp=True;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
def cube(n,p,s,mat='PP_ServiceEnamel',bevel=.006,rot=(0,0,0)):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p,rotation=rot);o=bpy.context.object;o.name=n;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);register(o,mat)
 if bevel:
  b=o.modifiers.new('Manufactured edge','BEVEL');b.width=bevel;b.segments=2;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=b.name);normals(o)
 return o
def cyl(n,p,r,h,mat='PP_MachinedSteel',axis=(0,0,1),verts=32,bevel=0):
 bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=h,location=p);o=bpy.context.object;o.name=n;o.rotation_euler=Vector(axis).to_track_quat('Z','Y').to_euler();register(o,mat)
 for f in o.data.polygons:f.use_smooth=len(f.vertices)==4
 if bevel:
  b=o.modifiers.new('Cast edge','BEVEL');b.width=bevel;b.segments=2;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=b.name);normals(o)
 return o
def rod(n,a,b,r,mat='PP_PipePaint',verts=24):
 d=Vector(b)-Vector(a);return cyl(n,(Vector(a)+Vector(b))/2,r,d.length,mat,d,verts)
def mesh(n,v,f,mat,smooth=True):
 me=bpy.data.meshes.new(n);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(n,me);bpy.context.collection.objects.link(o);register(o,mat)
 for face in me.polygons:face.use_smooth=smooth
 return o
def lathe(n,profile,p=(0,0,0),mat='PP_ReceiverPaint',segments=48):
 v=[];f=[]
 for r,z in profile:
  for i in range(segments):a=i*2*pi/segments;v.append((p[0]+r*cos(a),p[1]+r*sin(a),p[2]+z))
 for j in range(len(profile)-1):
  for i in range(segments):a=j*segments+i;b=j*segments+(i+1)%segments;f.append((a,b,b+segments,a+segments))
 f+=[tuple(reversed(range(segments))),tuple((len(profile)-1)*segments+i for i in range(segments))]
 return mesh(n,v,f,mat)
def tube(n,pts,r=.09,wall=.008,mat='PP_PipePaint',segments=24):
 # Real inner bore with end annuli; no black disk posing as a hole.
 pts=[Vector(p) for p in pts];v=[];f=[]
 frames=[];previous_u=None
 for j,p in enumerate(pts):
  t=(pts[min(j+1,len(pts)-1)]-pts[max(0,j-1)]).normalized()
  if previous_u is None:
   guide=Vector((0,0,1)) if abs(t.z)<.95 else Vector((0,1,0));u=t.cross(guide).normalized()
  else:u=(previous_u-t*previous_u.dot(t)).normalized()
  w=t.cross(u).normalized();frames.append((u,w));previous_u=u
 for rad in [r,r-wall]:
  for p,(u,w) in zip(pts,frames):
   for k in range(segments):a=k*2*pi/segments;v.append(tuple(p+rad*(cos(a)*u+sin(a)*w)))
 count=len(pts)*segments
 for side in [0,1]:
  off=side*count
  for j in range(len(pts)-1):
   for k in range(segments):a=off+j*segments+k;b=off+j*segments+(k+1)%segments;face=(a,b,b+segments,a+segments);f.append(tuple(reversed(face)) if side else face)
 for end in [0,len(pts)-1]:
  for k in range(segments):a=end*segments+k;b=end*segments+(k+1)%segments;face=(a,a+count,b+count,b);f.append(tuple(reversed(face)) if end else face)
 o=mesh(n,v,f,mat)
 for face in list(o.data.polygons)[-segments*2:]:face.use_smooth=False
 return o
def torus(n,p,r,t,mat='PP_MachinedSteel',axis=(0,0,1),major=40,minor=8):
 bpy.ops.mesh.primitive_torus_add(major_radius=r,minor_radius=t,major_segments=major,minor_segments=minor,location=p);o=bpy.context.object;o.name=n;o.rotation_euler=Vector(axis).to_track_quat('Z','Y').to_euler();register(o,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o
def bolt(n,p,axis=(0,0,1),r=.012,head=.010,mat='PP_FastenerZinc'):
 a=Vector(axis).normalized();p=Vector(p);cyl(n+' washer',p-a*.002,r*1.40,.003,mat,a,16);return cyl(n+' hex',p+a*head*.5,r,head,mat,a,6,.0007)
def flange(n,p,axis=(1,0,0),r=.09,outer=.145,thickness=.028,bolts=6,mat='PP_MachinedSteel'):
 a=Vector(axis).normalized();p=Vector(p);tube(n+' ring',[p-a*thickness/2,p+a*thickness/2],outer,outer-(r-.007),mat,32)
 guide=Vector((0,0,1)) if abs(a.z)<.95 else Vector((0,1,0));u=a.cross(guide).normalized();v=a.cross(u).normalized()
 for i in range(bolts):ang=(i+.5)*2*pi/bolts;bolt(n+' bolt',p+a*(thickness*.5)+(.5*(outer+r))*(u*cos(ang)+v*sin(ang)),a,r=.0085,head=.008)
def plate(n,p,s,normal=(0,-1,0)):
 # Riveted rating blank, no generated lettering or fake legibility.
 o=cube(n,p,s,'PP_PlateIvory',.002)
 for xx in [-.5,.5]:
  for zz in [-.5,.5]:cyl(n+' rivet',Vector(p)+Vector((xx*(s[0]-.018),-.5*s[1]-.001,zz*(s[2]-.013))),.0025,.003,'PP_MachinedSteel',normal,8)
 return o
def join(name,items):
 bpy.ops.object.select_all(action='DESELECT')
 for o in items:o.select_set(True)
 bpy.context.view_layer.objects.active=items[0];bpy.ops.object.join();o=bpy.context.object;o.name=name;scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 # Packed, non-overlapping UVs per final mesh, including custom tube/lathe meshes.
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.1519,island_margin=.015,correct_aspect=True,scale_to_bounds=True);bpy.ops.object.mode_set(mode='OBJECT')
 o.data.uv_layers.active.name='UVMap';return o
def export(name,notes,ports=None,groups=None,collider=None):
 global parts
 root=bpy.data.objects.new('EP_PP_'+name,None);bpy.context.collection.objects.link(root)
 if not groups:groups=[(name+'_Geometry',parts.copy())]
 for group,items in groups:o=join(group,items);o.parent=root
 objects=[root]+list(root.children_recursive);collection=bpy.data.collections.new('Asset / '+name);scene.collection.children.link(collection)
 for o in objects:
  for c in list(o.users_collection):c.objects.unlink(o)
  collection.objects.link(o)
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=root
 file='exports/EP_PP_'+name+'.fbx';bpy.ops.export_scene.fbx(filepath=os.path.join(BASE,file),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False,use_mesh_modifiers=True,path_mode='RELATIVE')
 coords=[o.matrix_world@v.co for o in objects if o.type=='MESH' for v in o.data.vertices];bounds=[[min(v[i] for v in coords),max(v[i] for v in coords)] for i in range(3)]
 manifest[name]={'file':file,'root':'floor centre' if name in ['Receiver','ServiceCabinet','MotorSkid'] else 'see ports and notes','bounds_blender_xyz':bounds,'dimensions_width_depth_height':[b-a for a,b in bounds],'materials':sorted({m.name for o in objects if o.type=='MESH' for m in o.data.materials if m}),'mesh_objects':[{'name':o.name,'pivot_blender_xyz':list(o.location),'vertices':len(o.data.vertices),'triangles':sum(len(f.vertices)-2 for f in o.data.polygons),'uv_layers':[l.name for l in o.data.uv_layers]} for o in objects if o.type=='MESH'],'ports_blender_xyz':ports or [],'collider_recommendation':collider,'notes':notes}
 for o in objects:o.hide_render=True;o.hide_set(True)
 roots.append(root);parts=[];print('EXPORTED',name,manifest[name]['dimensions_width_depth_height'],flush=True)

# Receiver: dished shell, welded bands, feet, hand-access cover and real nozzle bores.
shell_profile=[(max(.001,.6*cos(a)),.66-.38*sin(a)) for a in np.linspace(pi/2,0,17)]+[(.6,1.87)]+[(max(.001,.6*cos(a)),1.87+.37*sin(a)) for a in np.linspace(0,pi/2,17)[1:]]
lathe('Dished receiver shell',shell_profile)
for z in [.655,1.875]:torus('Restrained circumferential weld',(0,0,z),.599,.005,'PP_MachinedSteel')
for a in [pi/6,5*pi/6,3*pi/2]:
 x=.43*cos(a);y=.43*sin(a);cube('Receiver footpad',(x,y,.018),(.21,.21,.036),'PP_CastIron',.007)
 cube('Receiver leg',(x,y,.25),(.105,.105,.45),'PP_ReceiverPaint',.009)
 for dx in [-.066,.066]:bolt('Floor anchor',(x+dx,y,.038),r=.011,head=.011)
 # Small rust retained at foot/plinth, not randomly spread over every face.
 cube('Foot weathered joint',(x,y,.06),(.116,.115,.012),'PP_Oxidation',.002)
tube('Receiver inlet neck',[(.53,0,.84),(.76,0,.84)],.09,.010,'PP_ReceiverPaint')
flange('Receiver inlet',(.732,0,.84),(1,0,0))
tube('Receiver upper outlet',[(0,0,2.19),(0,0,2.35)],.09,.010,'PP_ReceiverPaint')
flange('Receiver upper outlet',(0,0,2.315),(0,0,1),thickness=.026)
# Circular bolted inspection cover stands proud of the curved tank without duplicate wheel.
cyl('Inspection neck',(0,-.59,1.18),.212,.16,'PP_ReceiverPaint',(0,-1,0),48,.006)
cyl('Inspection cover gasket',(0,-.68,1.18),.228,.014,'PP_Rubber',(0,-1,0),48)
cyl('Inspection cover',(0,-.695,1.18),.225,.025,'PP_ReceiverPaint',(0,-1,0),48,.006)
for i in range(8):
 a=i*pi/4;bolt('Inspection bolt',(.191*cos(a),-.711,1.18+.191*sin(a)),(0,-1,0),.011,.010)
plate('Receiver identification',(0,-.612,1.70),(.235,.021,.105))
tube('Drain neck',[(.29,-.32,.39),(.29,-.46,.39),(.29,-.46,.19)],.022,.004,'PP_MachinedSteel',16)
cyl('Drain service plug',(.29,-.46,.18),.031,.04,'PP_MachinedSteel',(0,0,1),6)
export('Receiver','Main pressure shell diameter 1.20 m. Total height 2.35 m; side nozzle and access cover enlarge envelope. Front inspection cover is -Y/+UnityZ. No decorative handwheel. No moving parts.',ports=[{'name':'inlet','position':[.76,0,.84],'outward':[1,0,0],'bore_diameter':.16},{'name':'outlet','position':[0,0,2.35],'outward':[0,0,1],'bore_diameter':.16}],collider='Simple vertical capsule/body plus feet ignored; side nozzle box only if reachable. Root floor Z=0.')

# Service housing: all visible construction lies INSIDE the existing baffle bounds.
cube('Continuous opaque service core',(0,0,1.38),(1.59,5.91,2.54),'PP_Recess',0)
cube('Full floor plinth',(0,0,.065),(1.7,6,.13),'PP_CastIron',.008)
cube('Top folded lip',(0,0,2.675),(1.7,6,.05),'PP_ServiceEnamel',.008)
for side in [-1,1]:
 x=side*.82
 for y in [-2.91,-.97,.97,2.91]:cube('Service upright',(x,y,1.38),(.055,.052,2.49),'PP_MachinedSteel',.005)
 for z in [.20,2.49]:cube('Service horizontal rail',(x,0,z),(.05,5.88,.055),'PP_MachinedSteel',.004)
 for index,y in enumerate([-1.94,0,1.94]):
  cube('Recessed panel gasket',(side*.803,y,1.36),(.023,1.846,2.22),'PP_Rubber',.002)
  cube('Removable service door',(side*.817,y,1.36),(.023,1.804,2.175),'PP_ReplacementPanel' if side==1 and index==1 else 'PP_ServiceEnamel',.008)
  # Small stamped lower break gives a second surface angle at grazing light.
  cube('Door lower fold',(side*.827,y,.455),(.012,1.75,.036),'PP_ServiceEnamel',.003)
  for zy in [.68,1.98]:
   cyl('Door hinge pin',(side*.825,y-.842,zy),.016,.105,'PP_MachinedSteel',(0,0,1),16)
   cube('Door hinge strap',(side*.832,y-.817,zy),(.022,.075,.055),'PP_MachinedSteel',.002)
  for zy in [.82,1.78]:
   cyl('Quarter-turn service latch',(side*.839,y+.72,zy),.018,.012,'PP_MachinedSteel',(side,0,0),16)
   cube('Latch slot',(side*.846,y+.72,zy),(.002,.020,.004),'PP_Recess',0)
  # Rating plate on one side bay, not rows of distracting labels.
  if index==0:cube('Small asset identification',(side*.835,y-.50,1.92),(.009,.245,.098),'PP_PlateIvory',.002)
  if index in [0,2]:
   for j in range(3):cube('Small worn lower paint edge',(side*.830,y-.68+j*.066,.293+j*.004),(.002,.032+.018*j,.003),'PP_Oxidation' if j==0 else 'PP_MachinedSteel',0)
 # Lower skirting, slight panel color differences remain restrained.
 cube('Lower service skirt',(side*.821,0,.155),(.027,5.81,.043),'PP_ServiceEnamel',.003)
# Near end is a framed return grille with actual slat depth and opaque dark backing.
for x in [-.756,.756]:cube('Front grille stile',(x,-2.97,1.40),(.064,.047,2.45),'PP_ServiceEnamel',.004)
for z in [.21,1.00,2.57]:cube('Front grille rail',(0,-2.97,z),(1.48,.047,.065),'PP_ServiceEnamel',.004)
cube('Lower filter hatch',(0,-2.974,.60),(1.424,.035,.70),'PP_ServiceEnamel',.007)
cube('Grille deep backing',(0,-2.936,1.79),(1.405,.015,1.48),'PP_Recess',.002)
for z in np.linspace(1.08,2.48,13):cube('Angled return louver',(0,-2.965,float(z)),(1.399,.044,.083),'PP_ServiceEnamel',.002,rot=(.32,0,0))
for x in [-.56,.56]:cyl('Filter hatch lock',(x,-2.996,.60),.017,.007,'PP_MachinedSteel',(0,-1,0),16)
# Far end is deliberately opaque: no new view of Echo through the enclosure.
cube('Far end cover',(0,2.973,1.36),(1.48,.035,2.38),'PP_ServiceEnamel',.008)
for x in [-.75,.75]:cube('Far corner seam',(x,2.97,1.36),(.048,.05,2.47),'PP_MachinedSteel',.004)
export('ServiceCabinet','Closed replacement skin for existing pressure baffle. Exact axis-aligned envelope width 1.70 m, long depth 6.00 m, height 2.70 m. Floor-centred root; grille/front at Blender Y=-3, intended Unity Z=+3. All sides and interior core are opaque. No ventilation opening provides a through sightline. Can retain existing 1.7x2.7x6 box collider. Remove/disable old visible baffle to avoid z-fighting; keep collision only.',collider='Existing baffle box: Unity centre (0,1.35,0), size (1.7,2.7,6).')

# Reusable flanged main pipe, 2m from mating plane to mating plane, axis X.
tube('Straight pipe bore',[(-1,0,0),(1,0,0)])
for x,s in [(-.971,-1),(.971,1)]:flange('Pipe end flange',(x,0,0),(s,0,0))
export('PipeStraight2m','Main collector: pipe OD 0.18 m, bore 0.164 m. Root at centre; mate at +/-1 m X. Hex fasteners remain inside mating planes. Hollow ends; assemble to another flange or wall connection, never leave hanging in space.',ports=[{'name':'A','position':[-1,0,0],'outward':[-1,0,0]},{'name':'B','position':[1,0,0],'outward':[1,0,0]}])

# 90 degree bend with 0.32 m centreline radius plus 0.18 m straight tangents.
pts=[(0,0,0),(.18,0,0)]+[(.18+.32*sin(a),.32*(1-cos(a)),0) for a in np.linspace(0,pi/2,17)[1:]]+[(.50,.50,0)]
tube('Swept elbow bore',pts)
flange('Elbow inlet',(.029,0,0),(-1,0,0));flange('Elbow outlet',(.5,.471,0),(0,1,0))
export('PipeElbow90','Origin is first port. 0.32 m centreline bend radius. Source inlet outward -X, outlet +Y; centreline mating ports (0,0,0) and (0.5,0.5,0). Rotate module as required; straight pipe/flange dimensions match.',ports=[{'name':'A','position':[0,0,0],'outward':[-1,0,0]},{'name':'B','position':[.5,.5,0],'outward':[0,1,0]}])

# Tee casting, genuinely cut bores; avoid intersecting closed cylinders inside junction.
main=cyl('Tee casting run',(0,0,0),.094,1,'PP_CastIron',(1,0,0),32)
branch=cyl('Tee casting branch',(0,.245,0),.094,.51,'PP_CastIron',(0,1,0),32)
bpy.context.view_layer.objects.active=main;mod=main.modifiers.new('Cast joined branch','BOOLEAN');mod.operation='UNION';mod.solver='EXACT';mod.object=branch;bpy.ops.object.modifier_apply(modifier=mod.name);parts.remove(branch);bpy.data.objects.remove(branch,do_unlink=True)
for n,p,axis,h in [('Main bore',(0,0,0),(1,0,0),1.10),('Branch bore',(0,.26,0),(0,1,0),.58)]:
 cut=cyl(n,p,.082,h,'PP_Recess',axis,32);bpy.context.view_layer.objects.active=main;mod=main.modifiers.new(n,'BOOLEAN');mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cut;bpy.ops.object.modifier_apply(modifier=mod.name);parts.remove(cut);bpy.data.objects.remove(cut,do_unlink=True)
for p,a in [((-.471,0,0),(-1,0,0)),((.471,0,0),(1,0,0)),((0,.471,0),(0,1,0))]:flange('Tee flange',p,a)
export('PipeTee','Cast collector tee with three open connected bores. Source main axis X, branch +Y. Root at branch intersection; main mating planes +/-0.5 X, branch +0.5 Y.',ports=[{'name':'A','position':[-.5,0,0],'outward':[-1,0,0]},{'name':'B','position':[.5,0,0],'outward':[1,0,0]},{'name':'C','position':[0,.5,0],'outward':[0,1,0]}])

# Band and standoff: origin matches pipe centreline, backing wall at +Y.
tube('Split pipe band',[(-.025,0,0),(.025,0,0)],.107,.011,'PP_MachinedSteel',32)
for x in [-.03,.03]:cube('Clamp tightening ear',(x,.095,0),(.022,.06,.08),'PP_MachinedSteel',.003)
bolt('Clamp transverse screw',(-.046,.107,0),(-1,0,0),.010,.012)
cube('Support stand off',(0,.176,0),(.065,.142,.052),'PP_CastIron',.003)
cube('Wall anchor plate',(0,.262,0),(.18,.03,.29),'PP_MachinedSteel',.006)
for z in [-.103,.103]:bolt('Masonry anchor',(0,.242,z),(0,-1,0),.011,.012)
export('PipeWallSupport','Pipe axis X through root. Mount wall plane at Blender Y=+0.277 (behind pipe); source up Z. Collar fits OD0.18 with a small assembly gap. No collision necessary.',ports=[{'name':'pipe axis','position':[0,0,0],'outward':[1,0,0]},{'name':'wall plane','position':[0,.277,0],'outward':[0,1,0]}])

# Short adaptor to bridge fixed control/motor height without stretching a 2m flange.
tube('Short pipe bore',[(-.15,0,0),(.15,0,0)])
for x,s in [(-.121,-1),(.121,1)]:flange('Adaptor flange',(x,0,0),(s,0,0))
export('PipeAdaptor300','0.30 m mating-plane length, axis X. Use as a short visible connection; no decorative wheel.',ports=[{'name':'A','position':[-.15,0,0],'outward':[-1,0,0]},{'name':'B','position':[.15,0,0],'outward':[1,0,0]}])

# A terminal cap makes unused branches physically closed, not free-cut pipes.
cyl('Blind flange plate',(.019,0,0),.145,.038,'PP_MachinedSteel',(1,0,0),32,.004)
for i in range(6):a=(i+.5)*pi/3;bolt('Blind flange bolt',(.042,.1175*cos(a),.1175*sin(a)),(1,0,0),.0085,.008)
cyl('Blind pressure test plug',(.052,0,0),.022,.022,'PP_MachinedSteel',(1,0,0),6,.001)
export('PipeBlindCap','Mating plane at X=0. Cap occupies +X; mating outward -X. Fits collector flange, closes unused branch. No wheel or operable detail.',ports=[{'name':'A','position':[0,0,0],'outward':[-1,0,0]}])

# Compact pump motor: evolved from Campaign motor grammar, original new mesh.
cube('Skid pan',(0,0,.055),(1.52,.80,.11),'PP_CastIron',.012)
for y in [-.29,.29]:cube('Structural runner',(0,y,.16),(1.37,.09,.13),'PP_MachinedSteel',.005)
for x in [-.61,.61]:
 for y in [-.29,.29]:bolt('Skid foundation bolt',(x,y,.229),r=.013,head=.015)
for x in [-.48,-.03]:
 cube('Motor mounting foot',(x,0,.274),(.16,.40,.10),'PP_MotorPaint',.008)
 for y in [-.14,.14]:bolt('Motor mounting bolt',(x,y,.33),r=.011,head=.013)
cyl('Cast motor body',(-.285,0,.535),.219,.64,'PP_MotorPaint',(1,0,0),48,.010)
# Longitudinal cooling fins parallel to rotor axis, unlike repeated torus toy coils.
for i in range(18):
 a=i*2*pi/18;y=.232*cos(a);z=.535+.232*sin(a)
 o=cube('Motor cooling vane',(-.285,y,z),(.52,.039,.014),'PP_MotorPaint',.003);o.rotation_euler.x=a
cyl('Rear fan housing',(-.655,0,.535),.245,.115,'PP_MotorPaint',(1,0,0),48,.012)
cyl('Rear fan recess',(-.716,0,.535),.188,.006,'PP_Recess',(-1,0,0),40)
for k in range(-4,5):
 z=.535+k*.033;extent=math.sqrt(max(.001,.18**2-(k*.033)**2));rod('Fan guard strip',(-.723,-extent,z),(-.723,extent,z),.006,'PP_MachinedSteel',12)
cyl('Front motor end',(.07,0,.535),.225,.07,'PP_MotorPaint',(1,0,0),48,.006)
cyl('Coupling guard',(.20,0,.535),.13,.23,'PP_MachinedSteel',(1,0,0),32)
for x in [.135,.19,.245]:torus('Coupling guard collar',(x,0,.535),.132,.006,'PP_CastIron',(1,0,0),32,6)
cube('Pump housing foot',(.445,0,.275),(.34,.31,.10),'PP_MotorPaint',.009)
cyl('Pump volute',(.445,0,.535),.245,.25,'PP_MotorPaint',(1,0,0),48,.009)
cyl('Pump casing cover',(.585,0,.535),.224,.047,'PP_MotorPaint',(1,0,0),48,.005)
for i in range(6):
 a=(i+.5)*pi/3;bolt('Pump cover bolt',(.614,.187*cos(a),.535+.187*sin(a)),(1,0,0),.010,.012)
tube('Pump axial inlet',[(.59,0,.535),(.79,0,.535)],.09,.008,'PP_MotorPaint');flange('Pump inlet',(.759,0,.535),(1,0,0))
tube('Pump upper discharge',[(.41,0,.70),(.41,0,1.01)],.09,.008,'PP_MotorPaint');flange('Pump discharge',(.41,0,.981),(0,0,1))
cube('Motor electrical box',(-.31,0,.822),(.26,.24,.14),'PP_MotorPaint',.018)
cube('Terminal box lid',(-.31,0,.902),(.28,.257,.026),'PP_MotorPaint',.007)
for x in [-.41,-.21]:
 for y in [-.086,.086]:bolt('Terminal lid screw',(x,y,.918),r=.0055,head=.005)
tube('Short wiring conduit',[(-.32,.13,.82),(-.32,.20,.82),(-.32,.26,.76),(-.32,.26,.22)],.013,.003,'PP_Rubber',12)
plate('Motor rating plate',(-.30,-.263,.543),(.23,.012,.091))
export('MotorSkid','Floor-centred compact pump and finned motor. No wheel, gauge, dial or light to compete with functional Finish control. Front is -Y/+UnityZ; motor shaft runs X. Externally connected suction and discharge are hollow. Electrical conduit terminates at skid frame. Use existing Finish control separately.',ports=[{'name':'axial inlet','position':[.79,0,.535],'outward':[1,0,0],'bore_diameter':.164},{'name':'upper discharge','position':[.41,0,1.01],'outward':[0,0,1],'bore_diameter':.164}],collider='Single box on skid/body about Unity (1.52,0.78,0.80), plus outlet if necessary. Never use triangle colliders on fins.')

with open(os.path.join(BASE,'manifest.json'),'w') as f:json.dump({'date':'2026-10-01','authorship':'Original project-authored mesh and optional paint maps. No external sources or paid dependencies. Motor uses the design vocabulary of prior project Campaign kit, newly constructed.','axis_contract':'Source Blender X width, -Y front, +Z up; FBX -Z/+Y convention. Current Unity importer without baking maps (-X,Z,-Y), including X reflection. Negate imported root localScale.x once, preserve imported rotation and scale magnitudes, then apply placementRotation * importedRotation to obtain the assembly contract (X,Z,-Y). No mesh/FBX re-export for the correction. See README and axis-diagnosis.json.','materials':MATERIALS,'assets':manifest},f,indent=2)
for o in [roots[0]]+list(roots[0].children_recursive):o.hide_set(False);o.hide_render=False
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'EchoPressurePlant.blend'))
for o in [roots[0]]+list(roots[0].children_recursive):o.hide_set(True);o.hide_render=True

# Studio previews, not Game-view evidence. Assets remain metric and unscaled.
scene.render.engine='CYCLES';scene.cycles.samples=20;scene.cycles.use_denoising=True;scene.render.resolution_x=1280;scene.render.resolution_y=960;scene.render.resolution_percentage=100;scene.world.color=(.1,.1,.1);scene.view_settings.view_transform='AgX'
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.005));ground=bpy.context.object;ground.name='Preview only ground';gm=bpy.data.materials.new('Preview floor');gm.diffuse_color=(.084,.09,.08,1);ground.data.materials.append(gm)
for name,p,power,size,color in [('Key',(-4,-5,7),1300,5,(1,.88,.73)),('Fill',(5,-2,5),700,4,(.69,.83,1)),('Back',(1,5,6),1200,4,(1,.93,.80))]:
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=size;d.color=color;o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,0,1))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();camera=bpy.context.object;camera.data.lens=45;scene.camera=camera
def show(names):
 for root in roots:
  state=root.name.replace('EP_PP_','') not in names
  for o in [root]+list(root.children_recursive):o.hide_render=state;o.hide_set(state)
def render(name,position,target,lens=45):
 camera.location=position;camera.rotation_euler=(Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.lens=lens;scene.render.filepath=os.path.join(PRE,name+'.png');bpy.ops.render.render(write_still=True)
only=os.environ.get('EP_PREVIEW_ONLY','')
if not only or 'Receiver' in only:
 show(['Receiver']);render('Receiver',(-2.7,-4.0,1.85),(0,0,1.15),48)
if not only or 'ServiceCabinet' in only:
 show(['ServiceCabinet']);render('ServiceCabinet',(6.6,-7.6,3.7),(0,0,1.33),43)
 render('ServiceCabinet-player',(3.2,-4.8,1.68),(0,-1.5,1.38),35)
if not only or 'MotorSkid' in only:
 show(['MotorSkid']);render('MotorSkid',(-1.95,-2.55,1.68),(0,0,.50),48)
if not only or 'Modules' in only:
 show(['PipeStraight2m','PipeElbow90','PipeTee','PipeWallSupport','PipeAdaptor300','PipeBlindCap'])
 placements={'PipeStraight2m':(0,.75,.18),'PipeElbow90':(-1.18,-.52,.18),'PipeTee':(.55,-.50,.18),'PipeWallSupport':(-.50,-1.05,.18),'PipeAdaptor300':(.25,-1.25,.18),'PipeBlindCap':(1.05,-1.12,.18)}
 for root in roots:
  key=root.name.replace('EP_PP_','')
  if key in placements:root.location=placements[key]
 render('PipeModules',(-2.4,-3.4,3.7),(0,0,.18),46)
 for root in roots:root.location=(0,0,0)
print('PRESSURE PLANT COMPLETE',OUT,flush=True)
