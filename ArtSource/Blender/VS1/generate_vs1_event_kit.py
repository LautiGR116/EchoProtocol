"""Build the first selective VS1 environment-source kit.

Run with Blender in background mode. The script saves the editable .blend,
exports one FBX per module, and renders contact-sheet style preview images.
All measurements are in metres and every module keeps its origin at floor level.
"""

from __future__ import annotations

import math
from pathlib import Path

import bpy
from mathutils import Vector


SOURCE_DIR = Path(__file__).resolve().parent
PROJECT_ROOT = SOURCE_DIR.parents[2]
EXPORT_DIR = PROJECT_ROOT / "Assets" / "_Game" / "Art" / "Models" / "VS1" / "EventKit"
PREVIEW_DIR = SOURCE_DIR / "Previews"
BLEND_PATH = SOURCE_DIR / "EP_VS1_EventKit_v01.blend"


def reset_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials, bpy.data.cameras, bpy.data.lights):
        for datablock in list(datablocks):
            if datablock.users == 0:
                datablocks.remove(datablock)


def material(name: str, color: tuple[float, float, float, float], metallic: float = 0.0,
             roughness: float = 0.55) -> bpy.types.Material:
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = color
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    return mat


def link_only(obj: bpy.types.Object, collection: bpy.types.Collection) -> None:
    for current in list(obj.users_collection):
        current.objects.unlink(obj)
    collection.objects.link(obj)


def cube(name: str, size: tuple[float, float, float], location: tuple[float, float, float],
         mat: bpy.types.Material, collection: bpy.types.Collection, bevel: float = 0.012) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    link_only(obj, collection)
    obj.data.materials.append(mat)
    if bevel > 0.0:
        modifier = obj.modifiers.new("Edge Softening", "BEVEL")
        modifier.width = bevel
        modifier.segments = 2
        modifier.limit_method = "ANGLE"
    return obj


def cylinder(name: str, radius: float, depth: float, location: tuple[float, float, float],
             rotation: tuple[float, float, float], mat: bpy.types.Material,
             collection: bpy.types.Collection, vertices: int = 20, bevel: float = 0.006) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=location,
                                       rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    link_only(obj, collection)
    obj.data.materials.append(mat)
    if bevel > 0.0:
        modifier = obj.modifiers.new("Edge Softening", "BEVEL")
        modifier.width = bevel
        modifier.segments = 2
    return obj


def new_collection(name: str) -> bpy.types.Collection:
    collection = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(collection)
    return collection


def build_door(mats: dict[str, bpy.types.Material]) -> bpy.types.Collection:
    c = new_collection("EP_VS1_DomesticDoor")
    wood = mats["wood"]
    wood_dark = mats["wood_dark"]
    trim = mats["painted_trim"]
    metal = mats["aged_metal"]

    cube("Door_Leaf", (1.00, 0.070, 2.15), (0.0, 0.0, 1.075), wood, c, 0.018)
    cube("Door_Jamb_L", (0.105, 0.145, 2.30), (-0.565, 0.0, 1.15), trim, c, 0.012)
    cube("Door_Jamb_R", (0.105, 0.145, 2.30), (0.565, 0.0, 1.15), trim, c, 0.012)
    cube("Door_Head", (1.235, 0.145, 0.105), (0.0, 0.0, 2.2475), trim, c, 0.012)
    cube("Door_Casing_L", (0.085, 0.055, 2.40), (-0.655, -0.075, 1.20), trim, c, 0.01)
    cube("Door_Casing_R", (0.085, 0.055, 2.40), (0.655, -0.075, 1.20), trim, c, 0.01)
    cube("Door_Casing_Head", (1.395, 0.055, 0.085), (0.0, -0.075, 2.3575), trim, c, 0.01)

    # Two restrained recessed panels communicate "domestic door" at a distance.
    cube("Door_Panel_Upper", (0.74, 0.018, 0.72), (0.0, -0.044, 1.61), wood_dark, c, 0.01)
    cube("Door_Panel_Upper_Inset", (0.64, 0.012, 0.62), (0.0, -0.057, 1.61), wood, c, 0.008)
    cube("Door_Panel_Lower", (0.74, 0.018, 0.50), (0.0, -0.044, 0.60), wood_dark, c, 0.01)
    cube("Door_Panel_Lower_Inset", (0.64, 0.012, 0.40), (0.0, -0.057, 0.60), wood, c, 0.008)

    cube("Door_Handle_Backplate", (0.075, 0.035, 0.22), (0.395, -0.070, 1.045), metal, c, 0.012)
    cylinder("Door_Handle_Hub", 0.045, 0.075, (0.395, -0.115, 1.065),
             (math.radians(90), 0.0, 0.0), metal, c)
    cube("Door_Handle_Lever", (0.19, 0.035, 0.035), (0.315, -0.153, 1.065), metal, c, 0.012)
    cylinder("Door_Keyhole", 0.017, 0.010, (0.395, -0.094, 0.990),
             (math.radians(90), 0.0, 0.0), wood_dark, c, vertices=16, bevel=0.002)

    for index, height in enumerate((0.40, 1.15, 1.90), start=1):
        cube(f"Door_Hinge_{index:02d}", (0.035, 0.035, 0.115), (-0.505, -0.060, height), metal, c, 0.008)

    cube("Door_Threshold", (1.18, 0.20, 0.035), (0.0, -0.015, 0.0175), metal, c, 0.008)
    return c


def build_radiator(mats: dict[str, bpy.types.Material]) -> bpy.types.Collection:
    c = new_collection("EP_VS1_DomesticRadiator")
    enamel = mats["radiator"]
    metal = mats["aged_metal"]
    dark = mats["wood_dark"]

    cube("Radiator_Back", (1.18, 0.095, 0.70), (0.0, 0.035, 0.56), dark, c, 0.012)
    for index in range(11):
        x = -0.50 + index * 0.10
        cube(f"Radiator_Fin_{index + 1:02d}", (0.064, 0.135, 0.64), (x, -0.025, 0.56), enamel, c, 0.022)
    cube("Radiator_Top", (1.18, 0.18, 0.065), (0.0, -0.005, 0.91), enamel, c, 0.015)
    cube("Radiator_Side_L", (0.08, 0.16, 0.68), (-0.575, -0.010, 0.56), enamel, c, 0.018)
    cube("Radiator_Side_R", (0.08, 0.16, 0.68), (0.575, -0.010, 0.56), enamel, c, 0.018)
    cylinder("Radiator_Valve", 0.065, 0.16, (0.69, -0.005, 0.31),
             (0.0, math.radians(90), 0.0), metal, c, vertices=16)
    cylinder("Radiator_Valve_Knob", 0.075, 0.095, (0.81, -0.005, 0.31),
             (0.0, math.radians(90), 0.0), dark, c, vertices=12)
    cylinder("Radiator_Pipe", 0.028, 0.31, (0.69, -0.005, 0.145),
             (0.0, 0.0, 0.0), metal, c, vertices=14)
    cube("Radiator_Foot_L", (0.12, 0.14, 0.10), (-0.40, 0.0, 0.14), metal, c, 0.014)
    cube("Radiator_Foot_R", (0.12, 0.14, 0.10), (0.40, 0.0, 0.14), metal, c, 0.014)
    return c


def build_observation_frame(mats: dict[str, bpy.types.Material]) -> bpy.types.Collection:
    c = new_collection("EP_VS1_ObservationFrame")
    metal = mats["frame_metal"]
    gasket = mats["gasket"]

    cube("Frame_Post_L", (0.095, 0.13, 3.05), (-0.96, 0.0, 1.525), metal, c, 0.010)
    cube("Frame_Post_R", (0.095, 0.13, 3.05), (0.96, 0.0, 1.525), metal, c, 0.010)
    cube("Frame_Head", (2.015, 0.13, 0.095), (0.0, 0.0, 3.0025), metal, c, 0.010)
    cube("Frame_Sill", (2.015, 0.16, 0.125), (0.0, 0.0, 0.0625), metal, c, 0.012)
    cube("Frame_Gasket_L", (0.026, 0.026, 2.82), (-0.895, -0.075, 1.525), gasket, c, 0.004)
    cube("Frame_Gasket_R", (0.026, 0.026, 2.82), (0.895, -0.075, 1.525), gasket, c, 0.004)
    cube("Frame_Gasket_Head", (1.82, 0.026, 0.026), (0.0, -0.075, 2.925), gasket, c, 0.004)
    cube("Frame_Gasket_Sill", (1.82, 0.026, 0.026), (0.0, -0.075, 0.14), gasket, c, 0.004)
    return c


def build_transition(mats: dict[str, bpy.types.Material]) -> bpy.types.Collection:
    c = new_collection("EP_VS1_FloorTransition")
    metal = mats["aged_metal"]
    wood = mats["wood"]
    dark = mats["wood_dark"]

    cube("Transition_Wood", (1.68, 0.26, 0.026), (0.0, 0.0, 0.013), wood, c, 0.006)
    cube("Transition_Edge_Clinical", (1.76, 0.045, 0.035), (0.0, -0.145, 0.0175), metal, c, 0.007)
    cube("Transition_Edge_Domestic", (1.76, 0.045, 0.035), (0.0, 0.145, 0.0175), metal, c, 0.007)
    for x in (-0.73, 0.73):
        cylinder("Transition_Fastener_L" if x < 0 else "Transition_Fastener_R", 0.018, 0.006,
                 (x, -0.145, 0.038), (0.0, 0.0, 0.0), dark, c, vertices=12, bevel=0.002)
    return c


def apply_modifiers(collections: list[bpy.types.Collection]) -> None:
    for collection in collections:
        for obj in collection.objects:
            if obj.type != "MESH":
                continue
            bpy.context.view_layer.objects.active = obj
            obj.select_set(True)
            for modifier in list(obj.modifiers):
                bpy.ops.object.modifier_apply(modifier=modifier.name)
            obj.select_set(False)


def export_collection(collection: bpy.types.Collection, filename: str) -> None:
    bpy.ops.object.select_all(action="DESELECT")
    for obj in collection.objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = next(iter(collection.objects))
    bpy.ops.export_scene.fbx(
        filepath=str(EXPORT_DIR / filename),
        use_selection=True,
        object_types={"MESH"},
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        use_space_transform=True,
        bake_space_transform=False,
        axis_forward="-Z",
        axis_up="Y",
        add_leaf_bones=False,
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        path_mode="AUTO",
    )


def look_at(obj: bpy.types.Object, target: tuple[float, float, float]) -> None:
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def add_preview_floor(mats: dict[str, bpy.types.Material]) -> bpy.types.Object:
    preview_collection = new_collection("Preview_Studio")
    floor = cube("Preview_Floor", (8.0, 7.0, 0.06), (0.0, 0.5, -0.045), mats["preview_floor"],
                 preview_collection, 0.0)
    return floor


def configure_render() -> tuple[bpy.types.Object, list[bpy.types.Object]]:
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1200
    scene.render.resolution_y = 900
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.world.color = (0.015, 0.020, 0.030)

    bpy.ops.object.camera_add(location=(4.6, -6.2, 3.6))
    camera = bpy.context.object
    camera.name = "Preview_Camera"
    camera.data.lens = 52
    camera.data.sensor_width = 36
    scene.camera = camera

    lights: list[bpy.types.Object] = []
    for name, location, energy, size, color in (
        ("Key", (-3.0, -4.0, 5.2), 1050.0, 4.0, (0.72, 0.84, 1.0)),
        ("Fill", (4.0, -1.0, 3.0), 700.0, 3.0, (1.0, 0.72, 0.50)),
        ("Rim", (0.0, 3.5, 4.5), 900.0, 2.5, (0.46, 0.64, 1.0)),
    ):
        bpy.ops.object.light_add(type="AREA", location=location)
        light = bpy.context.object
        light.name = f"Preview_{name}"
        light.data.energy = energy
        light.data.shape = "DISK"
        light.data.size = size
        light.data.color = color
        look_at(light, (0.0, 0.0, 1.0))
        lights.append(light)
    return camera, lights


def render_asset(collections: list[bpy.types.Collection], visible: list[bpy.types.Collection], camera: bpy.types.Object,
                 filename: str, camera_location: tuple[float, float, float], target: tuple[float, float, float]) -> None:
    for collection in collections:
        collection.hide_render = collection not in visible
    camera.location = camera_location
    look_at(camera, target)
    bpy.context.scene.render.filepath = str(PREVIEW_DIR / filename)
    bpy.ops.render.render(write_still=True)


def main() -> None:
    EXPORT_DIR.mkdir(parents=True, exist_ok=True)
    PREVIEW_DIR.mkdir(parents=True, exist_ok=True)
    bpy.context.preferences.filepaths.save_version = 0
    reset_scene()

    mats = {
        "wood": material("MAT_Domestic_Wood", (0.19, 0.070, 0.035, 1.0), roughness=0.48),
        "wood_dark": material("MAT_Domestic_WoodDark", (0.045, 0.025, 0.020, 1.0), roughness=0.60),
        "painted_trim": material("MAT_Domestic_PaintedTrim", (0.22, 0.24, 0.25, 1.0), roughness=0.72),
        "aged_metal": material("MAT_Domestic_AgedMetal", (0.075, 0.070, 0.060, 1.0), metallic=0.70, roughness=0.42),
        "radiator": material("MAT_Domestic_Radiator", (0.26, 0.28, 0.27, 1.0), metallic=0.18, roughness=0.66),
        "frame_metal": material("MAT_Observation_Frame", (0.035, 0.050, 0.065, 1.0), metallic=0.72, roughness=0.34),
        "gasket": material("MAT_Observation_Gasket", (0.006, 0.008, 0.010, 1.0), roughness=0.86),
        "preview_floor": material("MAT_Preview_Floor", (0.055, 0.065, 0.080, 1.0), roughness=0.75),
    }

    door = build_door(mats)
    radiator = build_radiator(mats)
    frame = build_observation_frame(mats)
    transition = build_transition(mats)
    assets = [door, radiator, frame, transition]
    apply_modifiers(assets)

    # Save the clean, origin-aligned source before building the temporary studio.
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))
    export_collection(door, "EP_VS1_DomesticDoor_v01.fbx")
    export_collection(radiator, "EP_VS1_DomesticRadiator_v01.fbx")
    export_collection(frame, "EP_VS1_ObservationFrame_v01.fbx")
    export_collection(transition, "EP_VS1_FloorTransition_v01.fbx")

    add_preview_floor(mats)
    camera, _lights = configure_render()
    studio = bpy.data.collections["Preview_Studio"]

    render_asset(assets, [door], camera, "EP_VS1_DomesticDoor_v01.png", (3.35, -4.70, 2.55), (0.0, 0.0, 1.15))
    render_asset(assets, [radiator], camera, "EP_VS1_DomesticRadiator_v01.png", (2.65, -3.60, 1.75), (0.0, 0.0, 0.52))
    render_asset(assets, [frame], camera, "EP_VS1_ObservationFrame_v01.png", (3.70, -5.40, 2.70), (0.0, 0.0, 1.45))
    render_asset(assets, [transition], camera, "EP_VS1_FloorTransition_v01.png", (2.25, -2.85, 1.45), (0.0, 0.0, 0.0))

    # Keep preview-only content out of the editable production source.
    bpy.data.collections.remove(studio, do_unlink=True)
    bpy.data.objects.remove(camera, do_unlink=True)
    for obj in list(bpy.data.objects):
        if obj.name.startswith("Preview_"):
            bpy.data.objects.remove(obj, do_unlink=True)
    for collection in assets:
        collection.hide_render = False
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))


if __name__ == "__main__":
    main()
