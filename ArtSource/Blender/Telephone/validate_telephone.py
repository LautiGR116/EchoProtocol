import importlib.util,json
from pathlib import Path
import bpy
from mathutils import Matrix,Vector
BASE=Path(__file__).resolve().parent
spec=importlib.util.spec_from_file_location('common',BASE.parent/'RecoveryOffice'/'validate_recovery_office.py');common=importlib.util.module_from_spec(spec);spec.loader.exec_module(common)
m=json.loads((BASE/'manifest.json').read_text());e=m['assets']['RotaryTelephone'];r=common.validate_asset('RotaryTelephone',e,BASE)
h=bpy.data.objects['TelephoneHandset'];rest=Vector(e['anchors_blender_xyz']['RotaryTelephone__HANDSET_REST']);target=Vector(e['anchors_blender_xyz']['RotaryTelephone__HANDSET_OFFHOOK'])
error=common.vector_error(h.matrix_world.translation,rest)
h.matrix_world=Matrix.Translation(target) @ Matrix.Rotation(1.5707963267948966,4,'Z') @ Matrix.Translation(-rest) @ h.matrix_world
bpy.context.view_layer.update();coords=[h.matrix_world@v.co for v in h.data.vertices]
r['handset']={'pivot_error_m':error,'offhook_min_z':min(p.z for p in coords),'offhook_x':[min(p.x for p in coords),max(p.x for p in coords)],'offhook_y':[min(p.y for p in coords),max(p.y for p in coords)]}
r['physical_pass']=error<.0001 and .0001<r['handset']['offhook_min_z']<.008
(BASE/'roundtrip-checks.json').write_text(json.dumps(r,indent=2)+'\n');print(json.dumps(r))
assert r['pass'] and r['physical_pass']
