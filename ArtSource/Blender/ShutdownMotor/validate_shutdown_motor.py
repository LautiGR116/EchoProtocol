"""Independent FBX roundtrip checks. Writes only validation.json beside source.

Run after the builder has completed; no Unity or original-asset mutation.
"""
from pathlib import Path
import bpy, bmesh, json, math, hashlib
from mathutils import Vector, Matrix

BASE = Path(__file__).resolve().parent
ROOT = BASE.parents[2]
manifest = json.loads((BASE / 'manifest.json').read_text())
asset = manifest['assets']['MotorSkid']
checks = []


def check(name, passed, details=None):
    checks.append({'name': name, 'pass': bool(passed), 'details': details})


def points(objects):
    return [o.matrix_world @ v.co for o in objects for v in o.data.vertices]


def bounds(ps):
    return [[min(p[i] for p in ps), max(p[i] for p in ps)] for i in range(3)]


def close(a, b, tolerance=0.0002):
    return len(a) == len(b) and all(abs(x-y) <= tolerance for x, y in zip(a, b))


def ray(objects, y, z):
    origin, direction = Vector((-1.1, y, z)), Vector((1, 0, 0))
    hits = []
    for obj in objects:
        inverse = obj.matrix_world.inverted()
        hit, pos, normal, index = obj.ray_cast(inverse @ origin,
            (inverse.to_3x3() @ direction).normalized())
        if hit:
            world = obj.matrix_world @ pos
            hits.append(((world-origin).length, obj.name, list(world)))
    return sorted(hits)[0] if hits else None


bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(BASE / asset['file']))
new_objects = list(bpy.context.scene.objects)
meshes = [o for o in new_objects if o.type == 'MESH']
by_name = {o.name: o for o in new_objects}
check('exact separate static and rotor meshes',
      sorted(o.name for o in meshes) == ['ShutdownMotorStatic', 'ShutdownRotor'])
for obj in meshes:
    mesh = obj.data
    check(obj.name + ' finite vertices/normals',
          all(math.isfinite(v) for p in mesh.vertices for v in (*p.co, *p.normal)))
    check(obj.name + ' finite UVs', len(mesh.uv_layers) > 0 and
          all(math.isfinite(v) for uv in mesh.uv_layers for item in uv.data for v in item.uv))
    check(obj.name + ' valid material assignments', len(mesh.materials) > 0 and
          all(m is not None for m in mesh.materials) and
          all(0 <= face.material_index < len(mesh.materials) for face in mesh.polygons))
    check(obj.name + ' finite material values', all(
          all(math.isfinite(v) for v in m.diffuse_color) and
          math.isfinite(m.roughness) and math.isfinite(m.metallic) for m in mesh.materials))
    expected = next(m for m in asset['mesh_objects'] if m['name'] == obj.name)
    triangles = sum(len(p.vertices)-2 for p in mesh.polygons)
    check(obj.name + ' triangles/pivot roundtrip',
          triangles == expected['triangles'] and
          close(obj.matrix_world.translation, expected['pivot_blender_xyz']),
          {'triangles': triangles, 'pivot': list(obj.matrix_world.translation)})

actual_bounds = bounds(points(meshes))
check('manifest bounds roundtrip', all(close(a,b) for a,b in zip(actual_bounds, asset['bounds_blender_xyz'])), actual_bounds)
check('same original external envelope', all(close(a,b) for a,b in zip(actual_bounds,
    [[-.76,.79],[-.40,.40],[0,1.01]])))
check('total triangles <= 20000', sum(len(p.vertices)-2 for o in meshes for p in o.data.polygons) <= 20000)
for name, expected in asset['anchors_blender_xyz'].items():
    actual = by_name.get(name)
    check('anchor ' + name, actual is not None and close(actual.matrix_world.translation, expected),
          list(actual.matrix_world.translation) if actual else None)

static = by_name['ShutdownMotorStatic']
rotor = by_name['ShutdownRotor']
wheel_points = points([rotor])
check('wheel fits actual cavity', all(-.706 < p.x < -.670 and
      math.hypot(p.y, p.z-.535) < .161 for p in wheel_points), bounds(wheel_points))
bm = bmesh.new(); bm.from_mesh(rotor.data)
check('rotor closed surface / no wire edges', all(e.is_manifold for e in bm.edges))
bm.free()
for y,z in [( .10,.5515),(-.10,.5515),(.08,.4855),(.08,.6175)]:
    hit = ray(meshes,y,z)
    check('open guard gap ray %.4f %.4f' % (y,z), hit and hit[1] == rotor.name, hit)
for y,z in [(.10,.535),(.218,.535)]:
    hit = ray(meshes,y,z)
    check('fixed guard / hood ray %.4f %.4f' % (y,z), hit and hit[1] == static.name, hit)

original_matrix = rotor.matrix_world.copy()
pivot = rotor.matrix_world.translation.copy()
rotor.matrix_world = Matrix.Translation(pivot) @ Matrix.Rotation(math.radians(95),4,'X') @ Matrix.Translation(-pivot) @ original_matrix
bpy.context.view_layer.update()
for y,z in [(.10,.5515),(-.10,.5515)]:
    hit = ray(meshes,y,z)
    check('rotated wheel remains exposed %.4f %.4f' % (y,z), hit and hit[1] == rotor.name, hit)
check('rotated wheel stays in envelope', all(-.706 < p.x < -.670 and
      math.hypot(p.y, p.z-.535) < .161 for p in points([rotor])))
rotor.matrix_world = original_matrix
bpy.context.view_layer.update()

# The original FBX is imported read-only for a geometric comparison. Ignore the
# explicitly rebuilt rear cavity; every point outside it must remain unchanged.
old_ids = set(o.as_pointer() for o in bpy.context.scene.objects)
bpy.ops.import_scene.fbx(filepath=str(ROOT / 'ArtSource/Blender/PressurePlant/exports/EP_PP_MotorSkid.fbx'))
original_meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH' and o.as_pointer() not in old_ids]


def protected(ps):
    def outside_cavity(p):
        return not (-.751 <= p.x <= -.597 and abs(p.y) <= .253 and .282 <= p.z <= .788)
    return {tuple(round(v,4) for v in p) for p in ps if outside_cavity(p)}


before = protected(points(original_meshes)); after = protected(points([static]))
check('original construction outside rear cavity unchanged', before == after,
      {'original_unique_points': len(before), 'new_unique_points': len(after),
       'missing': len(before-after), 'added': len(after-before)})
result = {'status': 'PASS' if all(c['pass'] for c in checks) else 'FAIL',
          'checks': checks, 'count': len(checks),
          'fbx_sha256': hashlib.sha256((BASE / asset['file']).read_bytes()).hexdigest(),
          'scope': 'Blender FBX roundtrip/geometry only; no Unity, rendered motion, audio or human playtest.'}
(BASE / 'validation.json').write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps({'status':result['status'],'count':len(checks),
                  'failures':[c for c in checks if not c['pass']]}),flush=True)
if result['status'] != 'PASS':
    raise RuntimeError('ShutdownMotor validation failed; inspect validation.json')
