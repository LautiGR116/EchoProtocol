"""Derived adult + 16-joint deformation rig. MakeHuman graphical sources are CC0.
No generated animation is exported. Runtime articulation consumes recorded pose.
"""
import bpy, math, json, hashlib
from pathlib import Path
from mathutils import Vector
from mathutils.kdtree import KDTree
ROOT=Path('/Users/lautigr116/EchoProtocol')
source=(ROOT/'ArtSource/Blender/Reconstruction/build_recorded_adult.py').read_text()
source=source.split('# Neutral review light')[0]
source=source.replace("OUT=ROOT/'ArtSource/Blender/Reconstruction'","OUT=ROOT/'ArtSource/Blender/Contraprueba'")
source=source.replace("threshold=7.34 if c.z<.25 else 7.91+.22*math.exp(-((abs(c.x)-.48)/.22)**2)","threshold=7.22 if c.z<.25 else 7.61+.28*math.exp(-((abs(c.x)-.48)/.22)**2)")
source=source.replace("bpy.ops.export_scene.fbx(filepath=str(OUT/'exports/EP_ReconstructedAdult.fbx'),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH'},bake_anim=False)","")
exec(compile(source,'derived_reconstruction','exec'))
surface=o;surface.name='RecordedAdultSurface'

def point(b,tail=False):
    d=skel['bones'][b]
    return bone_matrix(b)@joint(d['tail' if tail else 'head'])
defs={
 'Hips':(point('root'),point('spine03'),None),
 'Spine':(point('spine03'),point('neck01'),'Hips'),
 'Neck':(point('neck01'),point('head'),'Spine'),
 'Head':(point('head'),point('head',True),'Neck')}
for side in ['L','R']:
    defs['Arm.'+side]=(point('upperarm01.'+side),point('lowerarm01.'+side),'Spine')
    defs['Forearm.'+side]=(point('lowerarm01.'+side),point('wrist.'+side),'Arm.'+side)
    defs['Hand.'+side]=(point('wrist.'+side),point('wrist.'+side,True),'Forearm.'+side)
    defs['Thigh.'+side]=(point('upperleg01.'+side),point('lowerleg01.'+side),'Hips')
    defs['Shin.'+side]=(point('lowerleg01.'+side),point('foot.'+side),'Thigh.'+side)
    defs['Foot.'+side]=(point('foot.'+side),point('foot.'+side,True),'Shin.'+side)

def mapped(n):
    side=n[-2:] if n.endswith(('.L','.R')) else ''
    if n.startswith('upperarm'):return 'Arm'+side
    if n.startswith('lowerarm'):return 'Forearm'+side
    if n.startswith(('wrist','finger','metacarpal')):return 'Hand'+side
    if n.startswith('upperleg'):return 'Thigh'+side
    if n.startswith('lowerleg'):return 'Shin'+side
    if n.startswith(('foot','toe')):return 'Foot'+side
    if n.startswith('neck'):return 'Neck'
    if n in ['root','spine04'] or n.startswith('pelvis'):return 'Hips'
    if n.startswith(('spine','shoulder','clavicle','breast')):return 'Spine'
    return 'Head'

influences=[{} for _ in posed]
for name,entries in weights.items():
    target=mapped(name)
    for i,w in entries:
        if target in defs:influences[i][target]=influences[i].get(target,0)+w
ids=sorted({i for f in groups['body']+groups['helper-tights'] for i in f if influences[i]})
kd=KDTree(len(ids))
for i in ids:kd.insert(posed[i],i)
kd.balance()
groupsOut={n:surface.vertex_groups.new(name=n) for n in defs}
for v in surface.data.vertices:
    mixed={}
    for co,index,d in kd.find_n(v.co,4):
        factor=1/max(.001,d)**3
        for n,w in influences[index].items():mixed[n]=mixed.get(n,0)+w*factor
    best=sorted(mixed.items(),key=lambda p:-p[1])[:4];tot=sum(w for n,w in best)
    for n,w in best:groupsOut[n].add([v.index],w/tot,'REPLACE')

# Material-space albedo nuance follows the mesh under deformation.
colors=surface.data.color_attributes.new(name='SurfaceTint',type='BYTE_COLOR',domain='CORNER')
for p in surface.data.polygons:
    mat=surface.data.materials[p.material_index].name
    for li in p.loop_indices:
        v=surface.data.vertices[surface.data.loops[li].vertex_index].co
        c=[1.,1.,1.,1.]
        if mat=='FIG_Skin':
            cheek=math.exp(-((abs(v.x)-.055)/.029)**2-((v.z-1.68)/.034)**2)*max(0,min(1,(-v.y-.08)*10))
            c=[1.,1.-.10*cheek,1.-.11*cheek,1.]
        elif mat in ['FIG_Jacket','FIG_Trousers']:
            k=.94+.035*math.sin(v.z*39+v.x*24)*math.sin(v.y*31-v.z*14)
            c=[k,k,k,1.]
        colors.data[li].color=c

bpy.ops.object.select_all(action='DESELECT')
armData=bpy.data.armatures.new('RecordedAdultRig');arm=bpy.data.objects.new('RecordedAdultRig',armData);bpy.context.collection.objects.link(arm)
bpy.context.view_layer.objects.active=arm;arm.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
for n,(a,b,parent) in defs.items():
    eb=armData.edit_bones.new(n);eb.head=a;eb.tail=b if (b-a).length>.02 else a+Vector((0,0,.06))
    eb.use_deform=True;eb.roll=0
for n,(a,b,parent) in defs.items():
    if parent:armData.edit_bones[n].parent=armData.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT')
surface.parent=arm;mod=surface.modifiers.new('Recorded body deformation','ARMATURE');mod.object=arm
surface.select_set(True)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'EchoContraprueba.blend'))
dest=OUT/'exports/EP_RecordedAdultRig.fbx'
bpy.ops.export_scene.fbx(filepath=str(dest),use_selection=True,apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',axis_forward='-Z',axis_up='Y',object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=False,armature_nodetype='NULL')
manifest={'date':'2026-10-08','derivedFrom':'Reconstruction build_recorded_adult.py + pinned MakeHuman CC0 graphical source','vertices':len(surface.data.vertices),'triangles':sum(len(p.vertices)-2 for p in surface.data.polygons),'bones':list(defs),'authored':'Rest-pose mature adult, broadened receding hairline, quiet skin/cloth vertex color; bounded four-influence weights transferred from the official base','animation':'No clips. Runtime locomotion phase, local velocity, breathing phase and head pitch must be recorded before this rig replays them. No eye tracking, voice or interactions.','sha256':hashlib.sha256(dest.read_bytes()).hexdigest()}
(OUT/'actor-manifest.json').write_text(json.dumps(manifest,indent=2));print(json.dumps(manifest))
