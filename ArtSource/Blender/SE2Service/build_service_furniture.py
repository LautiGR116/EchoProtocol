"""Original metric SE2 service furniture. Source X width, -Y front, +Z up.
Only this source directory is written; no third-party geometry or textures.
"""
import bpy,math,json,hashlib
from pathlib import Path
from mathutils import Vector
BASE=Path(__file__).resolve().parent;OUT=BASE/'exports';OUT.mkdir(exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
palette={'PP_ServiceEnamel':((.285,.305,.271,1),.57,.12),'PP_MachinedSteel':((.25,.28,.26,1),.39,.8),'PP_Rubber':((.016,.021,.018,1),.87,0),'PP_Recess':((.021,.027,.024,1),.89,0),'PP_PlateIvory':((.49,.477,.373,1),.67,.05)}
mats={};parts=[];assets={}
for n,(c,r,m) in palette.items():
 t=bpy.data.materials.new(n);t.use_nodes=True;t.diffuse_color=c;p=t.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=c;p.inputs['Roughness'].default_value=r;p.inputs['Metallic'].default_value=m;mats[n]=t

def finish(o,n,mat,bevel=0):
 o.name=n;o.data.materials.append(mats[mat]);parts.append(o)
 if bevel:
  b=o.modifiers.new('Rolled manufactured edge','BEVEL');b.width=bevel;b.segments=3;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=b.name)
  for f in o.data.polygons:f.use_smooth=True
  w=o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');w.keep_sharp=True;bpy.ops.object.modifier_apply(modifier=w.name)
 return o

def box(n,p,s,mat='PP_ServiceEnamel',bevel=.006):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return finish(o,n,mat,bevel)
def rod(n,a,b,r,mat='PP_MachinedSteel',verts=12):
 d=Vector(b)-Vector(a);bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=d.length,location=(Vector(a)+Vector(b))/2);o=bpy.context.object;o.rotation_euler=d.to_track_quat('Z','Y').to_euler();return finish(o,n,mat)
def asset(name,notes):
 global parts
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name+'_Geometry';scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(island_margin=.012,scale_to_bounds=True);bpy.ops.object.mode_set(mode='OBJECT');o.data.uv_layers.active.name='UVMap'
 root=bpy.data.objects.new('EP_SE2_'+name,None);bpy.context.collection.objects.link(root);o.parent=root
 bpy.ops.object.select_all(action='DESELECT');o.select_set(True);root.select_set(True);bpy.context.view_layer.objects.active=root
 fn=OUT/('EP_SE2_'+name+'.fbx');bpy.ops.export_scene.fbx(filepath=str(fn),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False,use_mesh_modifiers=True,path_mode='RELATIVE')
 o.data.calc_loop_triangles();v=[o.matrix_world@p.co for p in o.data.vertices]
 assets[name]={'file':str(fn.relative_to(BASE)),'triangles':len(o.data.loop_triangles),'bounds_blender_xyz':[[min(p[i] for p in v),max(p[i] for p in v)] for i in range(3)],'materials':[m.name for m in o.data.materials],'notes':notes,'sha256':hashlib.sha256(fn.read_bytes()).hexdigest()};parts=[]

# Closed passive service case: no fan, light, control affordance or implied sound.
for x in (-.38,.38):
 for y in (-.38,.38):box('Rubber isolator',(x,y,.04),(.12,.12,.08),'PP_Rubber',.014)
box('Folded base',(0,0,.105),(.98,.98,.07),bevel=.014)
box('Enamel casing',(0,0,.605),(.96,.94,.94),bevel=.018)
box('Separated top gasket',(0,0,1.081),(.966,.946,.012),'PP_Rubber',.004)
box('Removable top',(0,0,1.105),(1,1,.03),bevel=.008)
for y in (-.476,.476):
 sign=1 if y>0 else -1
 box('Service panel shadow',(0,y,.59),(.77,.008,.64),'PP_Recess',.006)
 for i in range(14):
  z=.32+i*.038
  b=box('Folded ventilation louver',(0,y+sign*.006,z),(.72,.015,.018),'PP_ServiceEnamel',.003);b.rotation_euler.x=sign*math.radians(15)
 box('Upper inspection plate',(0,y+sign*.005,.985),(.54,.012,.11),'PP_PlateIvory',.004)
 for x in (-.32,.32):
  for z in (.27,.9):rod('Captive service screw',(x,y+sign*.005,z),(x,y+sign*.012,z),.006,verts=8)
for x in (-.488,.488):
 sign=1 if x>0 else -1
 for y in (-.16,.16):box('Recessed handle mount',(x,y,.85),(.008,.045,.1),'PP_Recess',.004)
 rod('Flush carry handle',(x,-.16,.86),(x,.16,.86),.012)
asset('ServiceCase','1.00 x1.00 x1.12m fixed occupied footprint corresponding to the domestic chair. Two-sided louvers, removable lid, fasteners and rubber feet. No function, motor, emission or sound.')

# The same waist-height occupied footprint becomes a domestic key console.
for x in (-.355,.355):
 for y in (-.155,.155):
  box('Square tube leg',(x,y,.3975),(.028,.028,.735),'PP_MachinedSteel',.003)
  box('Rubber foot',(x,y,.025),(.041,.041,.05),'PP_Rubber',.005)
box('Lower shelf',(0,0,.19),(.77,.37,.028),bevel=.005)
box('Inset top',(0,0,.83),(.82,.42,.04),bevel=.008)
for y in (-.185,.185):box('Folded top skirt',(0,y,.79),(.78,.018,.07),bevel=.004)
for x in (-.385,.385):box('Folded end skirt',(x,0,.79),(.018,.37,.07),bevel=.004)
box('Empty inspection tray',(0,.045,.859),(.30,.21,.018),'PP_PlateIvory',.006)
asset('ServiceTable','0.82 x0.42 x0.868m; open steel table, lower shelf, rubber feet and empty tray. Current Player contact only, no interaction.')
bpy.ops.wm.save_as_mainfile(filepath=str(BASE/'SE2_ServiceFurniture.blend'))
manifest={'authorship':'Original project geometry, no external assets','axis_contract':'X width, -Y front, Z up; retain importer rotation, reflect imported root X once, then apply scene yaw. Floor-centred root.','assets':assets}
(BASE/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n');print(json.dumps(manifest))
