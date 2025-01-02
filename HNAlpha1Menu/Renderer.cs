using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace HNAlpha1Menu
{
    public class Renderer : Overlay
    {
        Vector4 red = new Vector4(1, 0, 0, 255);
        Vector4 darkRed = new Vector4(0.5f, 0, 0, 255);

        Vector4 yellow = new Vector4(1, 1, 0, 255);

        Vector4 grey = new Vector4(0.4f, 0.4f, 0.4f, 255);
        Vector4 darkGrey = new Vector4(0.1f, 0.1f, 0.1f, 255);

        public bool fly = false;
        public float flySpeed = 10;
        public bool alter = false;
        public bool toneighbor = false;

        public bool freezeNeighbor = false;
        public bool freezeToPlayer = false;
        public Vector3 freezePos = new Vector3(0, 0, 0);
        public bool push = true;
        public float pushDistance = 150;

        public bool ambience = false;
        public bool neon = false;
        public float ambdist = 700;

        public bool ovspeed = false;
        public float walkspeed = 600;
        public float runspeed = 1200;

        public float basefov = 112;
        public bool dynfov = false;
        public bool bobbing = false;

        public bool wide = false;
        public float wideamm = 1.5f;

        public bool norotation = false;


        protected override void Render() // FUNCION de render, cada frame
        {

            ImGuiStylePtr style = ImGui.GetStyle();
            style.WindowBorderSize = 0;
            style.Colors[(int)ImGuiCol.Border] = darkRed;
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = darkRed;
            style.Colors[(int)ImGuiCol.TitleBgActive] = darkRed;
            style.Colors[(int)ImGuiCol.TitleBg] = darkRed;
            style.Colors[(int)ImGuiCol.FrameBg] = darkGrey;
            style.Colors[(int)ImGuiCol.CheckMark] = red;
            style.Colors[(int)ImGuiCol.ResizeGrip] = darkRed;

            ImGui.Begin("Welcome!");
            ImGui.TextColored(yellow, "Y to Hide/Show");
            ImGui.TextColored(yellow, "R to Reload");
            ImGui.Spacing();
            ImGui.Spacing();

            ImGui.Begin("Cheats");
            ImGui.TextColored(red, "-- FLY --");
            ImGui.Checkbox("Fly", ref fly);
            ImGui.InputFloat("Fly Speed", ref flySpeed);
            ImGui.TextColored(yellow, "V to enable Fly");
            ImGui.TextColored(grey, "X/Z for -/+ speed");
            ImGui.Checkbox("Alternative Mode", ref alter);
            ImGui.TextColored(grey, "(May mess physics)");
            ImGui.Checkbox("Tp to Neighbor", ref toneighbor);
            ImGui.Spacing();
            ImGui.Spacing();

            ImGui.TextColored(red, "-- Freeze --");
            ImGui.Checkbox("Freeze Neighbor", ref freezeNeighbor);
            ImGui.TextColored(yellow, "T to enable Freeze");
            ImGui.InputFloat3("Freeze at", ref freezePos);
            ImGui.TextColored(grey, "(0 means disabled)");
            ImGui.TextColored(grey, "(refreeze to update)");
            ImGui.Checkbox("Freeze relative to Player", ref freezeToPlayer);
            ImGui.TextColored(grey, "(may break AI pathfind)");
            ImGui.Checkbox("Push Neighbor", ref push);
            ImGui.InputFloat("Push Distance", ref pushDistance);
            ImGui.TextColored(grey, "(This makes you immortal)");
            ImGui.Spacing();
            ImGui.Spacing();

            ImGui.Begin("Features");
            ImGui.TextColored(red, "-- Proximity Ambience --");
            ImGui.Checkbox("Closeness Ambience", ref ambience);
            ImGui.InputFloat("Clossenes Distance", ref ambdist);
            ImGui.Checkbox("Ambience Neon", ref neon);
            ImGui.TextColored(grey, "(Adds tenssion when close to neighbor)");
            ImGui.Spacing();
            ImGui.Spacing();

            ImGui.TextColored(red, "-- Player Speed --");
            ImGui.Checkbox("Override Speeds", ref ovspeed);
            ImGui.InputFloat("Walk Speed", ref walkspeed);
            ImGui.InputFloat("Run Speed", ref runspeed);
            ImGui.Spacing();
            ImGui.Spacing();

            ImGui.TextColored(red, "-- FOV & Camera --");
            ImGui.InputFloat("Base FOV", ref basefov);
            ImGui.Checkbox("Dynamic Fov", ref dynfov);
            ImGui.Checkbox("View Bobbing", ref bobbing);
            ImGui.Checkbox("No Rotations", ref norotation);
            ImGui.Spacing();
            ImGui.Checkbox("Wide Mode", ref wide);
            ImGui.InputFloat("Wide Ammount", ref wideamm);
            ImGui.Spacing();
            ImGui.Spacing();
        }
    }
}