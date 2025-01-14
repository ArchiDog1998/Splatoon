﻿using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Utility.Raii;
using ECommons.Configuration;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using Splatoon.Structures;
using System.Reflection;

namespace Splatoon.Gui;

unsafe class OverlayGui : IDisposable
{
    readonly Splatoon p;
    int uid = 0;
    public OverlayGui(Splatoon p)
    {
        this.p = p;
        Svc.PluginInterface.UiBuilder.Draw += Draw;
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= Draw;
    }

    void Draw()
    {
        if (p.Profiler.Enabled) p.Profiler.Gui.StartTick();
        try
        {
            if (!Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                && !Svc.Condition[ConditionFlag.WatchingCutscene78])
            {
                uid = 0;
                if (p.Config.segments > 1000 || p.Config.segments < 4)
                {
                    p.Config.segments = 100;
                    p.Log("Your smoothness setting was unsafe. It was reset to 100.");
                }
                if (p.Config.lineSegments > 50 || p.Config.lineSegments < 4)
                {
                    p.Config.lineSegments = 20;
                    p.Log("Your line segment setting was unsafe. It was reset to 20.");
                }
                try
                {
                    void Draw()
                    {
                        foreach (var element in p.displayObjects)
                        {
                            if (element is DisplayObjectCircle elementCircle)
                            {
                                DrawRingWorld(elementCircle);
                            }
                            else if (element is DisplayObjectDot elementDot)
                            {
                                DrawPoint(elementDot);
                            }
                            else if (element is DisplayObjectText elementText)
                            {
                                DrawTextWorld(elementText);
                            }
                            else if (element is DisplayObjectLine elementLine)
                            {
                                DrawLineWorld(elementLine);
                            }
                            else if (element is DisplayObjectRect elementRect)
                            {
                                DrawRectWorld(elementRect);
                            }
                            else if(element is DisplayObjectDonut elementDonut)
                            {
                                DrawDonutWorld(elementDonut);
                            }
                        }
                    }

                    ImGuiHelpers.ForceNextWindowMainViewport();
                    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                    ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero);
                    ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size);
                    ImGui.Begin("Splatoon scene", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar
                        | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.AlwaysUseWindowPadding);
                    if (P.Config.SplatoonLowerZ)
                    {
                        CImGui.igBringWindowToDisplayBack(CImGui.igGetCurrentWindow());
                    }
                    if (P.Config.RenderableZones.Count == 0 || !P.Config.RenderableZonesValid)
                    {
                        Draw();
                    }
                    else
                    {
                        foreach (var e in P.Config.RenderableZones)
                        {
                            //var trans = e.Trans != 1.0f;
                            //if (trans) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, e.Trans);
                            ImGui.PushClipRect(new Vector2(e.Rect.X, e.Rect.Y), new Vector2(e.Rect.Right, e.Rect.Bottom), false);
                            Draw();
                            ImGui.PopClipRect();
                            //if(trans)ImGui.PopStyleVar();
                        }
                    }
                    ImGui.End();
                    ImGui.PopStyleVar();
                }
                catch (Exception e)
                {
                    p.Log("Splatoon exception: please report it to developer", true);
                    p.Log(e.Message, true);
                    p.Log(e.StackTrace, true);
                }
            }
        }
        catch (Exception e)
        {
            p.Log("Caught exception: " + e.Message, true);
            p.Log(e.StackTrace, true);
        }
        if (p.Profiler.Enabled) p.Profiler.Gui.StopTick();
    }

    internal Vector3 TranslateToScreen(double x, double y, double z)
    {
        Vector2 tenp;
        Svc.GameGui.WorldToScreen(
            new Vector3((float)x, (float)y, (float)z),
            out tenp
        );
        return new Vector3(tenp.X, tenp.Y, (float)z);
    }

    private void DrawDonutWorld(DisplayObjectDonut elementDonut)
    {
        Vector3 v1, v2, v3, v4;
        var outerradiuschonk = elementDonut.radius + elementDonut.donut;
        v1 = TranslateToScreen(
            elementDonut.x + (elementDonut.radius * Math.Sin((Math.PI / 23.0) * 0)),
            elementDonut.z,
            elementDonut.y + (elementDonut.radius * Math.Cos((Math.PI / 24.0) * 0))
        );
        v4 = TranslateToScreen(
            elementDonut.x + (outerradiuschonk * Math.Sin((Math.PI / 24.0) * 0)),
            elementDonut.z,
            elementDonut.y + (outerradiuschonk * Math.Cos((Math.PI / 24.0) * 0))
        );
        for (int i = 0; i <= 47; i++)
        {
            v2 = TranslateToScreen(
                elementDonut.x + (elementDonut.radius * Math.Sin((Math.PI / 24.0) * (i + 1))),
                elementDonut.z,
                elementDonut.y + (elementDonut.radius * Math.Cos((Math.PI / 24.0) * (i + 1)))
            );
            v3 = TranslateToScreen(
                elementDonut.x + (outerradiuschonk * Math.Sin((Math.PI / 24.0) * (i + 1))),
                elementDonut.z,
                elementDonut.y + (outerradiuschonk * Math.Cos((Math.PI / 24.0) * (i + 1)))
            );
            ImGui.GetWindowDrawList().PathLineTo(new Vector2(v1.X, v1.Y));
            ImGui.GetWindowDrawList().PathLineTo(new Vector2(v2.X, v2.Y));
            ImGui.GetWindowDrawList().PathLineTo(new Vector2(v3.X, v3.Y));
            ImGui.GetWindowDrawList().PathLineTo(new Vector2(v4.X, v4.Y));
            ImGui.GetWindowDrawList().PathLineTo(new Vector2(v1.X, v1.Y));
            ImGui.GetWindowDrawList().PathFillConvex(
                elementDonut.color
            );
            v1 = v2;
            v4 = v3;
        }
    }

    void DrawLineWorld(DisplayObjectLine e)
    {
        if (p.Profiler.Enabled) p.Profiler.GuiLines.StartTick();
        var result = GetAdjustedLine(new Vector3(e.ax, e.ay, e.az), new Vector3(e.bx, e.by, e.bz));
        if (result.posA == null) return;
        ImGui.GetWindowDrawList().PathLineTo(new Vector2(result.posA.Value.X, result.posA.Value.Y));
        ImGui.GetWindowDrawList().PathLineTo(new Vector2(result.posB.Value.X, result.posB.Value.Y));
        ImGui.GetWindowDrawList().PathStroke(e.color, ImDrawFlags.None, e.thickness);
        if (p.Profiler.Enabled) p.Profiler.GuiLines.StopTick();
    }

    (Vector2? posA, Vector2? posB) GetAdjustedLine(Vector3 pointA, Vector3 pointB)
    {
        var resultA = Svc.GameGui.WorldToScreen(new Vector3(pointA.X, pointA.Z, pointA.Y), out Vector2 posA);
        if (!resultA && !p.DisableLineFix)
        {
            var posA2 = GetLineClosestToVisiblePoint(pointA,
            (pointB - pointA) / p.CurrentLineSegments, 0, p.CurrentLineSegments);
            if (posA2 == null)
            {
                if (p.Profiler.Enabled) p.Profiler.GuiLines.StopTick();
                return (null, null);
            }
            else
            {
                posA = posA2.Value;
            }
        }
        var resultB = Svc.GameGui.WorldToScreen(new Vector3(pointB.X, pointB.Z, pointB.Y), out Vector2 posB);
        if (!resultB && !p.DisableLineFix)
        {
            var posB2 = GetLineClosestToVisiblePoint(pointB,
            (pointA - pointB) / p.CurrentLineSegments, 0, p.CurrentLineSegments);
            if (posB2 == null)
            {
                if (p.Profiler.Enabled) p.Profiler.GuiLines.StopTick();
                return (null, null);
            }
            else
            {
                posB = posB2.Value;
            }
        }

        return (posA, posB);
    }

    void DrawRectWorld(DisplayObjectRect e) //oof
    {
        if (p.Profiler.Enabled) p.Profiler.GuiLines.StartTick();
        var result1 = GetAdjustedLine(new Vector3(e.l1.ax, e.l1.ay, e.l1.az), new Vector3(e.l1.bx, e.l1.by, e.l1.bz));
        if (result1.posA == null) goto Alternative;
        var result2 = GetAdjustedLine(new Vector3(e.l2.ax, e.l2.ay, e.l2.az), new Vector3(e.l2.bx, e.l2.by, e.l2.bz));
        if (result2.posA == null) goto Alternative;
        goto Build;
    Alternative:
        result1 = GetAdjustedLine(new Vector3(e.l1.ax, e.l1.ay, e.l1.az), new Vector3(e.l2.ax, e.l2.ay, e.l2.az));
        if (result1.posA == null) goto Quit;
        result2 = GetAdjustedLine(new Vector3(e.l1.bx, e.l1.by, e.l1.bz), new Vector3(e.l2.bx, e.l2.by, e.l2.bz));
        if (result2.posA == null) goto Quit;
        Build:
        ImGui.GetWindowDrawList().AddQuadFilled(
            new Vector2(result1.posA.Value.X, result1.posA.Value.Y),
            new Vector2(result1.posB.Value.X, result1.posB.Value.Y),
            new Vector2(result2.posB.Value.X, result2.posB.Value.Y),
            new Vector2(result2.posA.Value.X, result2.posA.Value.Y), e.l1.color
            );
    Quit:
        if (p.Profiler.Enabled) p.Profiler.GuiLines.StopTick();
    }

    Vector2? GetLineClosestToVisiblePoint(Vector3 currentPos, Vector3 delta, int curSegment, int numSegments)
    {
        if (curSegment > numSegments) return null;
        var nextPos = currentPos + delta;
        if (Svc.GameGui.WorldToScreen(new Vector3(nextPos.X, nextPos.Z, nextPos.Y), out Vector2 pos))
        {
            var preciseVector = GetLineClosestToVisiblePoint(currentPos, (nextPos - currentPos) / p.Config.lineSegments, 0, p.Config.lineSegments);
            return preciseVector.HasValue ? preciseVector.Value : pos;
        }
        else
        {
            return GetLineClosestToVisiblePoint(nextPos, delta, ++curSegment, numSegments);
        }
    }

    public void DrawTextWorld(DisplayObjectText e)
    {
        if (Svc.GameGui.WorldToScreen(
                        new Vector3(e.x, e.z, e.y),
                        out Vector2 pos))
        {
            DrawText(e, pos);
        }
    }

    public void DrawText(DisplayObjectText e, Vector2 pos)
    {
        using var padding = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(5, 5));
        using var rounding = ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, 10f);
        using var bgColor = ImRaii.PushColor(ImGuiCol.ChildBg, ImGui.ColorConvertU32ToFloat4(e.bgcolor));
        using var textColor = ImRaii.PushColor(ImGuiCol.Text, e.fgcolor);
        using var font = ImRaii.PushFont(GetFont(ImGui.GetFontSize() * e.fscale));

        var size = ImGui.CalcTextSize(e.text);
        size = new Vector2(size.X + 10f, size.Y + 10f);

        ImGui.SetNextWindowPos(new Vector2(pos.X - size.X / 2, pos.Y - size.Y / 2));

        using var child = ImRaii.Child("##child" + e.text + ++uid, size, false,
            ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav
            | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysUseWindowPadding);

        ImGuiEx.Text(e.text);
    }

    unsafe static ImFontPtr GetFont(float size)
    {
        var style = new Dalamud.Interface.GameFonts.GameFontStyle(Dalamud.Interface.GameFonts.GameFontStyle.GetRecommendedFamilyAndSize(Dalamud.Interface.GameFonts.GameFontFamily.Axis, size));

        var handle = Svc.PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(style);

        try
        {
            var font = handle.Lock().ImFont;

            if ((IntPtr)font.NativePtr == IntPtr.Zero)
            {
                return ImGui.GetFont();
            }
            
            font.Scale = size / font.FontSize;
            return font;
        }
        catch
        {
            return ImGui.GetFont();
        }
    }

    public void DrawRingWorld(DisplayObjectCircle e)
    {
        int seg = p.Config.segments / 2;
        Svc.GameGui.WorldToScreen(new Vector3(
            e.x + e.radius * (float)Math.Sin(p.CamAngleX),
            e.z,
            e.y + e.radius * (float)Math.Cos(p.CamAngleX)
            ), out Vector2 refpos);
        var visible = false;
        Vector2?[] elements = new Vector2?[p.Config.segments];
        for (int i = 0; i < p.Config.segments; i++)
        {
            visible = Svc.GameGui.WorldToScreen(
                new Vector3(e.x + e.radius * (float)Math.Sin(Math.PI / seg * i),
                e.z,
                e.y + e.radius * (float)Math.Cos(Math.PI / seg * i)
                ),
                out Vector2 pos)
                || visible;
            if (pos.Y > refpos.Y || P.Config.NoCircleFix) elements[i] = new Vector2(pos.X, pos.Y);
        }
        if (visible)
        {
            foreach (var pos in elements)
            {
                if (pos == null) continue;
                ImGui.GetWindowDrawList().PathLineTo(pos.Value);
            }

            if (e.filled)
            {
                ImGui.GetWindowDrawList().PathFillConvex(e.color);
            }
            else
            {
                ImGui.GetWindowDrawList().PathStroke(e.color, ImDrawFlags.Closed, e.thickness);
            }
        }
    }

    public void DrawPoint(DisplayObjectDot e)
    {
        if (Svc.GameGui.WorldToScreen(new Vector3(e.x, e.z, e.y), out Vector2 pos))
            ImGui.GetWindowDrawList().AddCircleFilled(
            new Vector2(pos.X, pos.Y),
            e.thickness,
            ImGui.GetColorU32(e.color),
            100);
    }

    /*void DrawPolygon(DisplayObjectPolygon p)
    {
        var i = 0;
        var objects = new List<Action>();
        var coords = GetPolygon(p.e.Polygon.Select((x) =>
        {
            p.MemoryManager.WorldToScreen(new Vector3(x.X, x.Z, x.Y), out Vector2 pos);
            return pos;
        }).ToList());
        var medium = new Vector2(coords.Average(x => x.v2.X), coords.Average(x => x.v2.Y));
        DrawText(new DisplayObjectText(0, 0, 0, $"{medium.X}, {medium.Y}", 0xff000000, 0xff0000ff), medium);
        foreach (var c in coords)
        {
            ImGui.GetWindowDrawList().PathLineTo(c.v2);
            var txt = i.ToString();
            objects.Add(() => DrawText(new DisplayObjectText(0,0,0, $"{txt}: {c.v2.X}, {c.v2.Y}, {c.angle}", 0xff000000, 0xffffffff), c.v2));
            i++;
        }
        //ImGui.GetWindowDrawList().PathLineTo(coords.First().v2);

        if (p.e.Filled)
        {
            ImGui.GetWindowDrawList().PathFillConvex(p.e.color);
        }
        else
        {
            ImGui.GetWindowDrawList().PathStroke(p.e.color, ImDrawFlags.Closed, p.e.thicc);
        }
        foreach (var o in objects) o();
    }*/

    //Without test!
    public void DrawRingWorldNew(DisplayObjectDonut e)
    {
        var ptsInside = GetCircle(e.x, e.y, e.z, e.radius, p.Config.segments);
        var ptsOutside = GetCircle(e.x, e.y, e.z, e.radius + e.donut, p.Config.segments);

        var length = ptsInside.Length;
        for (int i = 0; i < length; i++)
        {
            var p1 = ptsInside[i];
            var p2 = ptsOutside[i];
            var p3 = ptsOutside[(i + 1) % length];
            var p4 = ptsInside[(i + 1) % length];

            DrawToScreen(GetPtsOnScreen(new Vector3[] { p1, p2, p3, p4 }), true, e.color);
        }
    }

    //An example of it. 
    //Try to use it in the core code! I didn't test this method.
    public void DrawRingWorldNew(DisplayObjectCircle e)
    {
        var pts3 = GetCircle(e.x, e.y, e.z, e.radius, p.Config.segments);
        var screenPts2 = GetPtsOnScreen(pts3);
        DrawToScreen(screenPts2, e.filled, e.color, e.thickness);
    }

    private void DrawToScreen(IEnumerable<Vector2> pts, bool filled, uint color, float thickness = 1)
    {
        foreach (var pt in pts)
        {
            ImGui.GetWindowDrawList().PathLineTo(pt);
        }

        if (filled)
        {
            ImGui.GetWindowDrawList().PathFillConvex(color);
        }
        else
        {
            ImGui.GetWindowDrawList().PathStroke(color, ImDrawFlags.Closed, thickness);
        }
    }


    public Vector3[] GetCircle(float x, float y, float z, float radius, int segment)
    {
        int seg = segment / 2;

        Vector3[] elements = new Vector3[segment];

        for (int i = 0; i < segment; i++)
        {
            elements[i] = new Vector3(
                x + radius * (float)Math.Sin(Math.PI / seg * i),
                z,
                y + radius * (float)Math.Cos(Math.PI / seg * i)
                );
        }

        return elements;
    }

    #region API for cut thing behind the camera. These are core code which passed tests.
    private static IEnumerable<Vector2> GetPtsOnScreen(IEnumerable<Vector3> pts)
    {
        var cameraPts = pts.Select(WorldToCamera).ToArray();
        var changedPts = new List<Vector3>(cameraPts.Length * 2);

        for (int i = 0; i < cameraPts.Length; i++)
        {
            var pt1 = cameraPts[i];
            var pt2 = cameraPts[(i + 1) % cameraPts.Length];

            if (pt1.Z > 0 && pt2.Z <= 0)
            {
                GetPointOnPlane(pt1, ref pt2);
            }
            if (pt2.Z > 0 && pt1.Z <= 0)
            {
                GetPointOnPlane(pt2, ref pt1);
            }

            if (changedPts.Count > 0 && Vector3.Distance(pt1, changedPts[changedPts.Count - 1]) > 0.001f)
            {
                changedPts.Add(pt1);
            }

            changedPts.Add(pt2);
        }

        return changedPts.Where(p => p.Z > 0).Select(p =>
        {
            CameraToScreen(p, out var screenPos, out _);
            return screenPos;
        });
    }

    const float PLANE_Z = 0.001f;
    private static void GetPointOnPlane(Vector3 front, ref Vector3 back)
    {
        if (front.Z < 0) return;
        if (back.Z > 0) return;

        var ratio = (PLANE_Z - back.Z) / (front.Z - back.Z);
        back.X = (front.X - back.X) * ratio + back.X;
        back.Y = (front.Y - back.Y) * ratio + back.Y;
        back.Z = PLANE_Z;
    }

    #region These are the api in the future... Maybe. https://github.com/goatcorp/Dalamud/pull/1203
    static readonly FieldInfo _matrix = Svc.GameGui.GetType().GetRuntimeFields().FirstOrDefault(f => f.Name == "getMatrixSingleton");
    private static unsafe Vector3 WorldToCamera(Vector3 worldPos)
    {
        var matrix = (MulticastDelegate)_matrix.GetValue(Svc.GameGui);
        var matrixSingleton = (IntPtr)matrix.DynamicInvoke();

        var viewProjectionMatrix = *(Matrix4x4*)(matrixSingleton + 0x1b4);
        return Vector3.Transform(worldPos, viewProjectionMatrix);
    }

    private static unsafe bool CameraToScreen(Vector3 cameraPos, out Vector2 screenPos, out bool inView)
    {
        screenPos = new Vector2(cameraPos.X / MathF.Abs(cameraPos.Z), cameraPos.Y / MathF.Abs(cameraPos.Z));
        var windowPos = ImGuiHelpers.MainViewport.Pos;

        var device = Device.Instance();
        float width = device->Width;
        float height = device->Height;

        screenPos.X = (0.5f * width * (screenPos.X + 1f)) + windowPos.X;
        screenPos.Y = (0.5f * height * (1f - screenPos.Y)) + windowPos.Y;

        var inFront = cameraPos.Z > 0;
        inView = inFront &&
                 screenPos.X > windowPos.X && screenPos.X < windowPos.X + width &&
                 screenPos.Y > windowPos.Y && screenPos.Y < windowPos.Y + height;

        return inFront;
    }
    #endregion
    #endregion
}
