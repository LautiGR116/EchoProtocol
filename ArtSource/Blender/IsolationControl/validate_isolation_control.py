"""Independent Blender FBX roundtrip and physical moving-part visibility checks.

Run in a disposable background Blender only after builder exports are stable.
Reuses the already verified finite geometry/UV/material/bounds/anchor validator;
adds this kit's pivot, animated endpoints, grip, dial and label visibility tests.
No .blend or FBX is saved and no Unity files are touched.
"""
import hashlib
import importlib.util
import json
import math
from pathlib import Path

import bpy
from mathutils import Matrix, Quaternion, Vector
from mathutils.bvhtree import BVHTree

BASE = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location('recovery_roundtrip', BASE.parent / 'RecoveryOffice' / 'validate_recovery_office.py')
common = importlib.util.module_from_spec(spec)
spec.loader.exec_module(common)
EXPECTED = ('IsolationPanel', 'ControlPedestal', 'PressureGauge')
EXPECTED_MESHES = {
    'IsolationPanel': {'IsolationHousing', 'IsolationLever'},
    'ControlPedestal': {'PedestalStructure', 'PedestalConduit'},
    'PressureGauge': {'PressureGaugeHousing', 'PressureNeedle'},
}
TOLERANCE = 0.0001


def world_tree():
    verts, faces, owners = [], [], []
    for obj in bpy.context.scene.objects:
        if obj.type != 'MESH':
            continue
        obj.data.calc_loop_triangles()
        offset = len(verts)
        verts.extend(obj.matrix_world @ vertex.co for vertex in obj.data.vertices)
        for triangle in obj.data.loop_triangles:
            faces.append(tuple(offset + index for index in triangle.vertices))
            owners.append(obj.name)
    return BVHTree.FromPolygons(verts, faces, all_triangles=True), owners


def ray_to_front(target, expected_mesh):
    tree, owners = world_tree()
    origin = Vector((target.x, -0.6, target.z))
    point, normal, index, distance = tree.ray_cast(origin, Vector((0, 1, 0)), 0.85)
    owner = owners[index] if index is not None else None
    return {'pass': owner == expected_mesh, 'target_blender_xyz': list(target),
            'first_hit_mesh': owner, 'expected_mesh': expected_mesh,
            'distance_m': float(distance) if index is not None else None,
            'hit_blender_xyz': list(point) if index is not None else None}


def animated_checks(name, entry):
    data = entry['animation']
    mesh = bpy.data.objects[data['mesh']]
    pivot = Vector(data['pivot_blender_xyz'])
    axis = Vector(data['axis_blender_xyz']).normalized()
    rest_matrix = mesh.matrix_world.copy()
    anchors = entry['anchors_blender_xyz']
    results = {'pivot_error_m': common.vector_error(mesh.matrix_world.translation, pivot), 'states': []}
    if name == 'IsolationPanel':
        high_name, low_name = 'LEVER_GRIP_MARCHA', 'LEVER_GRIP_AISLADO'
        high_key, low_key = 'marcha_degrees', 'aislado_degrees'
        radius, expected_owner = 0.27, 'IsolationLever'
    else:
        high_name, low_name = 'NEEDLE_TIP_HIGH', 'NEEDLE_TIP_ZERO'
        high_key, low_key = 'high_degrees', 'zero_degrees'
        radius, expected_owner = 0.05, 'PressureNeedle'
    high = Vector(anchors[name + '__' + high_name])
    low = Vector(anchors[name + '__' + low_name])
    # A point expressed in the imported mesh's local space is transformed with
    # its actual matrix, independently of static root-owned endpoint anchors.
    high_mesh_local = rest_matrix.inverted() @ high
    high_surface_vector = high - pivot
    surface_sample = pivot + high_surface_vector.normalized() * radius
    for label, degrees, expected_endpoint in [('rest', data[high_key], high), ('actuated', data[low_key], low)]:
        rotation = Quaternion(axis, math.radians(degrees))
        around = Matrix.Translation(pivot) @ rotation.to_matrix().to_4x4() @ Matrix.Translation(-pivot)
        mesh.matrix_world = around @ rest_matrix
        bpy.context.view_layer.update()
        actual_endpoint = mesh.matrix_world @ high_mesh_local
        endpoint_error = common.vector_error(actual_endpoint, expected_endpoint)
        target = around @ surface_sample
        visibility = ray_to_front(target, expected_owner)
        state = {'state': label, 'angle_degrees': degrees, 'endpoint_error_m': endpoint_error,
                 'endpoint_blender_xyz': list(actual_endpoint), 'grip_or_needle_front_ray': visibility,
                 'pass': endpoint_error <= TOLERANCE and visibility['pass']}
        if name == 'IsolationPanel':
            state['isolated_label_front_ray'] = ray_to_front(Vector(anchors[name + '__LABEL_AISLADO']), 'IsolationHousing')
            state['pass'] = state['pass'] and state['isolated_label_front_ray']['pass']
        results['states'].append(state)
    mesh.matrix_world = rest_matrix
    bpy.context.view_layer.update()
    results['pass'] = results['pivot_error_m'] <= TOLERANCE and all(state['pass'] for state in results['states'])
    return results


def main():
    path = BASE / 'manifest.json'
    manifest = json.loads(path.read_text())
    results = []
    for name in EXPECTED:
        try:
            entry = manifest['assets'][name]
            result = common.validate_asset(name, entry, BASE)
            checks = result['checks']
            if set(obj.name for obj in bpy.context.scene.objects if obj.type == 'MESH') != EXPECTED_MESHES[name]:
                result['failures'].append('unexpected mesh split for the declared animated parts')
            if not all('pivot_blender_xyz' in mesh for mesh in entry['mesh_objects']):
                result['failures'].append('every mesh must declare its source pivot')
            if not checks.get('explicit_root_anchor_present'):
                result['failures'].append('missing explicit ROOT anchor')
            if name == 'ControlPedestal':
                checks['floor_z_minus_1_5_error_m'] = abs(result['bounds_blender_xyz'][2][0] + 1.5)
                if checks['floor_z_minus_1_5_error_m'] > TOLERANCE:
                    result['failures'].append('pedestal does not reach floor 1.5m below control root')
            else:
                checks['animation_and_visibility'] = animated_checks(name, entry)
                if not checks['animation_and_visibility']['pass']:
                    result['failures'].append('moving part pivot, endpoint or frontal visibility failed')
            result['pass'] = not result['failures']
        except Exception as error:
            result = {'asset': name, 'pass': False, 'error': str(error)}
        results.append(result)
        print(name, 'PASS' if result['pass'] else 'FAIL', flush=True)
    total = sum(result.get('triangles', 0) for result in results)
    names_match = set(manifest['assets']) == set(EXPECTED)
    report = {
        'scope': 'Independent disposable Blender FBX reimport: exact mesh split, finite vertices/UVs/materials, metric bounds and anchors, declared pivot positions, total triangle budget, lever/gauge endpoint transforms, frontal grip/needle visibility at both states, isolated label visibility, and pedestal floor contact. No Unity import, in-game collision/ray interaction, labels/text rendering, audio, or human perception approval.',
        'manifest_sha256': hashlib.sha256(path.read_bytes()).hexdigest(),
        'exact_three_asset_names': names_match,
        'total_triangles': total, 'maximum_triangles': 20000,
        'triangle_budget_pass': 0 < total <= 20000,
        'manifest_triangle_total_matches': total == manifest['total_triangles'],
        'results': results,
        'all_pass': names_match and 0 < total <= 20000 and total == manifest['total_triangles'] and all(result['pass'] for result in results),
    }
    output = BASE / 'roundtrip-checks.json'
    output.write_text(json.dumps(common.json_safe(report), indent=2, allow_nan=False) + '\n')
    print('ALL PASS', report['all_pass'], 'TRIANGLES', total, flush=True)
    if not report['all_pass']:
        raise RuntimeError('IsolationControl roundtrip validation failed: ' + str(output))


if __name__ == '__main__':
    main()
