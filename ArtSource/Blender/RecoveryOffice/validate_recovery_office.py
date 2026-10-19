"""Blender FBX roundtrip and the ReviewMonitor's actual display opening.

Run in a disposable Blender background session after all three exports and the
manifest are stable. This script never saves the working .blend or edits Assets.
Default output: roundtrip-checks.json next to this validator.
"""
import argparse
import hashlib
import json
import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector
from mathutils.bvhtree import BVHTree


EXPECTED_ASSETS = ("ReviewDesk", "TaskLamp", "ReviewMonitor")
METRIC_TOLERANCE = 0.0001
MAX_TRIANGLES = 20000
BASE = Path(__file__).resolve().parent


def finite(values):
    return all(math.isfinite(float(value)) for value in values)


def vector_error(actual, expected):
    if len(expected) != 3 or not finite(actual) or not finite(expected):
        return float("inf")
    return max(abs(float(actual[index]) - float(expected[index])) for index in range(3))


def clear_import_scene():
    # Only the disposable in-memory Blender scene is cleared.
    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    for collection in (bpy.data.meshes, bpy.data.materials):
        for item in list(collection):
            if item.users == 0:
                collection.remove(item)


def material_is_finite(material):
    values = list(material.diffuse_color) + [material.metallic, material.roughness]
    if not finite(values):
        return False
    if material.use_nodes and material.node_tree:
        for node in material.node_tree.nodes:
            for socket in node.inputs:
                if not hasattr(socket, "default_value"):
                    continue
                value = socket.default_value
                if isinstance(value, (int, float)):
                    if not math.isfinite(float(value)):
                        return False
                elif getattr(socket, "type", "") in ("VECTOR", "RGBA"):
                    if not finite(value):
                        return False
    return True


def monitor_opening(meshes):
    """Nine world-space rays include the edges of the exact .69 x .38 display."""
    vertices, triangles, owners = [], [], []
    for obj in meshes:
        mesh = obj.data
        mesh.calc_loop_triangles()
        offset = len(vertices)
        vertices.extend(obj.matrix_world @ vertex.co for vertex in mesh.vertices)
        for triangle in mesh.loop_triangles:
            triangles.append(tuple(offset + index for index in triangle.vertices))
            owners.append(obj.name)
    if not triangles or not all(finite(vertex) for vertex in vertices):
        return {"pass": False, "error": "No finite triangulated monitor geometry for ray checks.", "rays": []}
    tree = BVHTree.FromPolygons(vertices, triangles, all_triangles=True, epsilon=0.0)
    rays = []
    for x in (-0.345, 0.0, 0.345):
        for z in (-0.19, 0.0, 0.19):
            origin = Vector((x, -0.25, z))
            # One micrometre includes a face coincident with the target plane.
            point, normal, triangle_index, distance = tree.ray_cast(origin, Vector((0.0, 1.0, 0.0)), 0.250001)
            hit = triangle_index is not None
            rays.append({
                "x_m": x, "z_m": z, "clear_to_display_plane": not hit,
                "hit_mesh": owners[triangle_index] if hit else None,
                "hit_triangle": triangle_index,
                "hit_distance_m": float(distance) if hit else None,
                "hit_world_xyz": list(point) if hit else None,
            })
    return {
        "pass": all(ray["clear_to_display_plane"] for ray in rays),
        "origin_y_m": -0.25, "display_plane_y_m": 0.0,
        "display_width_m": 0.69, "display_height_m": 0.38,
        "endpoint_tolerance_m": 0.000001, "rays": rays,
    }


def validate_asset(name, entry, source_dir):
    path = source_dir / entry["file"]
    if not path.is_file() or path.suffix.lower() != ".fbx":
        raise ValueError("Missing FBX export: " + str(path))
    clear_import_scene()
    bpy.ops.import_scene.fbx(filepath=str(path))
    objects = list(bpy.context.scene.objects)
    by_name = {obj.name: obj for obj in objects}
    meshes = [obj for obj in objects if obj.type == "MESH"]
    if not meshes or any(len(obj.data.vertices) == 0 for obj in meshes):
        raise ValueError("Import contains no geometry, or an empty mesh.")

    failures = []
    coordinates = [obj.matrix_world @ vertex.co for obj in meshes for vertex in obj.data.vertices]
    finite_vertices = all(finite(point) for point in coordinates)
    if not finite_vertices:
        failures.append("non-finite transformed vertices")
    bounds = [[min(point[index] for point in coordinates), max(point[index] for point in coordinates)] for index in range(3)]
    expected_bounds = entry["bounds_blender_xyz"]
    bounds_error = max(abs(bounds[index][edge] - expected_bounds[index][edge]) for index in range(3) for edge in range(2))
    if not math.isfinite(bounds_error) or bounds_error > METRIC_TOLERANCE:
        failures.append("bounds differ from manifest")

    expected_meshes = {mesh["name"]: mesh for mesh in entry["mesh_objects"]}
    actual_names = {obj.name for obj in meshes}
    if len(expected_meshes) != len(entry["mesh_objects"]) or actual_names != set(expected_meshes):
        failures.append("mesh name/count mismatch")
    mesh_results, material_names = [], set()
    total_triangles = 0
    for obj in meshes:
        mesh = obj.data
        mesh.calc_loop_triangles()
        triangle_count = len(mesh.loop_triangles)
        total_triangles += triangle_count
        finite_uvs = bool(mesh.uv_layers) and all(
            len(layer.data) == len(mesh.loops) and all(finite(loop.uv) for loop in layer.data)
            for layer in mesh.uv_layers
        )
        material_slots_valid = bool(mesh.materials) and all(material is not None for material in mesh.materials)
        material_indices_valid = all(0 <= polygon.material_index < len(mesh.materials) for polygon in mesh.polygons)
        finite_materials = all(material_is_finite(material) for material in mesh.materials if material)
        material_names.update(material.name for material in mesh.materials if material)
        expected_mesh = expected_meshes.get(obj.name, {})
        triangles_match = triangle_count == expected_mesh.get("triangles")
        pivot_expected = expected_mesh.get("pivot_blender_xyz", expected_mesh.get("pivot"))
        pivot_error = vector_error(obj.matrix_world.translation, pivot_expected) if pivot_expected is not None else None
        if not finite_uvs:
            failures.append(obj.name + ": absent or non-finite UVs")
        if not material_slots_valid or not material_indices_valid or not finite_materials:
            failures.append(obj.name + ": invalid material slots, indices or numeric values")
        if not triangles_match:
            failures.append(obj.name + ": triangle count differs from manifest")
        if pivot_error is not None and pivot_error > METRIC_TOLERANCE:
            failures.append(obj.name + ": pivot differs from manifest")
        mesh_results.append({
            "name": obj.name, "vertices": len(mesh.vertices), "triangles": triangle_count,
            "triangles_match_manifest": triangles_match, "finite_uvs": finite_uvs,
            "uv_layers": [layer.name for layer in mesh.uv_layers],
            "material_slots_valid": material_slots_valid, "material_indices_valid": material_indices_valid,
            "finite_materials": finite_materials, "pivot_error_m": pivot_error,
        })

    missing_materials = sorted(set(entry["materials"]) - material_names)
    unexpected_materials = sorted(material_names - set(entry["materials"]))
    if missing_materials or unexpected_materials:
        failures.append("material names differ from manifest")
    anchor_errors = {}
    for key, expected in entry["anchors_blender_xyz"].items():
        obj = by_name.get(key)
        anchor_errors[key] = vector_error(obj.matrix_world.translation, expected) if obj else float("inf")
    if not anchor_errors or any(error > METRIC_TOLERANCE for error in anchor_errors.values()):
        failures.append("missing, non-finite or displaced anchors")

    checks = {}
    if name == "ReviewDesk":
        checks["floor_contact_error_m"] = abs(bounds[2][0])
        desktop_anchor = by_name.get(name + "__DESKTOP_CENTRE")
        # Grommet/cable may rise above the tabletop; they are not its surface.
        checks["desktop_top_error_m"] = abs(desktop_anchor.matrix_world.translation.z - 0.760) if desktop_anchor else float("inf")
        if max(checks.values()) > METRIC_TOLERANCE:
            failures.append("desk floor/top contract failed")
    elif name == "TaskLamp":
        checks["cord_extends_below_support_plane"] = bounds[2][0] < -METRIC_TOLERANCE
        checks["lowest_geometry_z_m"] = bounds[2][0]
        if not checks["cord_extends_below_support_plane"]:
            failures.append("lamp cord does not extend below the table support plane")
    elif name == "ReviewMonitor":
        checks["foot_contact_error_m"] = abs(bounds[2][0] + 0.720)
        checks["display_opening"] = monitor_opening(meshes)
        if checks["foot_contact_error_m"] > METRIC_TOLERANCE:
            failures.append("monitor foot does not reach source Z=-.720")
        if not checks["display_opening"]["pass"]:
            failures.append("monitor casing obstructs an authored display ray")
    root_anchor = name + "__ROOT"
    checks["explicit_root_anchor_present"] = root_anchor in entry["anchors_blender_xyz"]
    if checks["explicit_root_anchor_present"]:
        checks["root_origin_error_m"] = vector_error(by_name[root_anchor].matrix_world.translation, (0, 0, 0)) if root_anchor in by_name else float("inf")
        if checks["root_origin_error_m"] > METRIC_TOLERANCE:
            failures.append("explicit asset root is not at the source origin")

    return {
        "asset": name, "file": entry["file"], "bytes": path.stat().st_size,
        "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
        "pass": not failures, "failures": failures, "mesh_count": len(meshes),
        "triangles": total_triangles, "finite_vertices": finite_vertices,
        "bounds_blender_xyz": bounds, "bounds_error_m": bounds_error,
        "missing_materials": missing_materials, "unexpected_materials": unexpected_materials,
        "anchor_errors_m": anchor_errors, "meshes": mesh_results, "checks": checks,
    }


def json_safe(value):
    if isinstance(value, float) and not math.isfinite(value):
        return None
    if isinstance(value, dict):
        return {key: json_safe(item) for key, item in value.items()}
    if isinstance(value, (list, tuple)):
        return [json_safe(item) for item in value]
    return value


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--manifest", type=Path, default=BASE / "manifest.json")
    parser.add_argument("--output", type=Path, default=BASE / "roundtrip-checks.json")
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    manifest = json.loads(args.manifest.read_text())
    assets = manifest["assets"]
    results = []
    names_match = set(assets) == set(EXPECTED_ASSETS)
    for name in EXPECTED_ASSETS:
        try:
            result = validate_asset(name, assets[name], args.manifest.parent)
        except Exception as error:
            result = {"asset": name, "pass": False, "error": str(error)}
        results.append(result)
        print(name, "PASS" if result["pass"] else "FAIL", flush=True)
    total_triangles = sum(result.get("triangles", 0) for result in results)
    triangle_budget_pass = total_triangles <= MAX_TRIANGLES
    report = {
        "scope": "Blender FBX roundtrip: metric bounds, named geometry/anchors, declared pivots, finite vertices/UVs/materials, aggregate triangle budget, and nine world-space rays through the exact monitor display opening. Root identity is checked when explicit ROOT anchors or mesh pivots are declared. Not Unity integration, in-game readability, runtime text alignment, physical collision, human visual or audio approval.",
        "all_pass": names_match and triangle_budget_pass and all(result["pass"] for result in results),
        "exact_three_asset_names": names_match, "total_triangles": total_triangles,
        "maximum_triangles": MAX_TRIANGLES, "triangle_budget_pass": triangle_budget_pass,
        "results": results,
    }
    args.output.write_text(json.dumps(json_safe(report), indent=2, allow_nan=False) + "\n")
    print("ALL PASS", report["all_pass"], "TRIANGLES", total_triangles, flush=True)
    if not report["all_pass"]:
        raise RuntimeError("RecoveryOffice FBX validation failed; see " + str(args.output))


if __name__ == "__main__":
    main()
