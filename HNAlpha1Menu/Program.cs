using HNAlpha1Menu;
using ImGuiNET;
using Microsoft.VisualBasic;
using Swed64;
using System.Data;
using System.Drawing;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml; // 2D and 3D Coords (Vectors?)


// ====================== INSTANTIATE RENDER ====================== //
Renderer renderer = new Renderer(); // instancia la clase del render, para que inicie el minimenu
renderer.Start().Wait(); // hace que esa instancia se inicie (corra)

// ====================== CONSOLE CONFIG ====================== //
Console.SetWindowSize(50, 35);
Console.SetWindowPosition(0, 0);
Console.Title = "Log Console";


// ====================== PROCESS VARIABLES ====================== //
Swed ram;
IntPtr physX;
IntPtr game;
IntPtr engineMovement;
IntPtr engineRotation;
bool attached = false;


// ====================== STATIC POINTERS AND OFFSETS OF VARIABLES ====================== //
int poiPlayer = 0x023930D0;
int poiPlayer2 = 0xF8;
int poiPlayer3 = 0x2A0;
int poiPosX = 0x120;
int poiPosZ = 0x124;
int poiPosY = 0x128;
int poiForceX = 0x62C;
int poiForceZ = 0x630;
int poiForceY = 0x634;

int poiScripts = 0x02315E40;
int poiNeighbor = 0x78;
int poiNeighborPos = 0x160;
int poiNeighborX = 0x120;
int poiNeighborZ = 0x124;
int poiNeighborY = 0x128;
int poiNeighborWIDE = 0x11C; // idk how does this work but its funny af lmao
int poiSpeed = 0x90;
int poiSpeed2 = 0x3B0;
int poiSpeed3 = 0x194;
int poiAcceleration = 0x1A8; // DEFAULT IS (2048 iirc...)
int poiCrouchHeight = 0x1D8; // DEFAULT IS 55
int poiCanMove = 0x170; // DEFAULT IS 1 (ITS AN 4BYTES, NOT A FLOAT) (maybe value??, looks more like a isGrounded value idk)

int poiCamera = 0x02399C40;
int poiCamProperties = 0x30;
int poiCamDisplay = 0x3D8;
int poiCamFov = 0x364;
int poiCamX = 0x378;
int poiCamY = 0x37C;
int poiCamZ = 0x380;

int poiPostprocess = 0x0238CC60;
int poiPostprocess2 = 0x8;
int poiColors = 0x3B0;
int poiRed = 0x28;
int poiGreen = 0x2C;
int poiBlue = 0x30;


// ====================== ACTUAL VARIABLES IN CODE ====================== //

// KEYS
bool spaceKey;
bool shiftKey;
bool controlKey;
bool vKey;
bool wKey;
bool aKey;
bool sKey;
bool dKey;
bool xKey;
bool zKey;
bool tKey;
bool rKey;


// PLAYER POSITION
nint posX;
nint posY;
nint posZ;
nint forceX;
nint forceY;
nint forceZ;
nint speed;
float currPosX;
float currPosY;
float currPosZ;
float lastY = 0;
Vector3 spawnPos = new Vector3(112.1f, 278.1f, 6295.1f);


// CAMERA
nint camX; //estos son solo pointers, no los valores. esta asi para que luego se pueda writear
nint camY;
nint camZ;
nint camFov;
Vector3 crowbarPos;
float finalFov = renderer.basefov;
Vector3 vAngles;

// NEIGHBOR POS
nint nPosX;
nint nPosY;
nint nPosZ;
nint nWide;
float oldNPosX = 0;
float oldNPosY = 0;
float oldNPosZ = 0;
float normdist = 0;
float distance;
Vector3 direction;

// POST PROCESSING COLORS
nint cRed;
nint cGreen;
nint cBlue;
Vector3 ogColor = Vector3.Zero;

// ITEMS
nint physics = 0x00284BF8;
nint items = 0x638;

nint it_crowbar = 0x2C0;
nint it_crowbarY = 0xC28;

nint it_lockpick = 0x3C8;
nint it_lockpickY = 0x7D8;

nint it_hammer = 0x2D0;
nint it_hammerY = 0xD98;

nint it_jackhammer = 0x2C0;
nint it_jackhammerY = 0xD98;

//MISC VARIABLES
double degreeToRadian = Math.PI / 180;
double anglesOffset = 90 * degreeToRadian;
bool dbFly = false;
bool dbFreeze = false;
bool dbAttach = false;
float time = 0;


// -------------------------------------------- MAIN LOOP -------------------------------------------- //

Attach();
UpdateConsole();
void UpdateConsole()
{
    while (true) //MAIN LOOP
    {
        handleAttach();
        if (attached)
        {
            time++;
            Variables();
            Speed();
            if (canUpdate())
            {
                Fly();
                Ambience();
                Bobbing();
                DynamicFov();
            }
            Freeze();
            ToNeighbor();
            if (canUpdate()){UpdateFov();}
            WideMode();
            NoRotationMode();
            Push();
        }
        Thread.Sleep(0);
    }
}

bool canUpdate(){return time % 30 == 0;}

// -------------------------------------------- ATTACH PROCESS -------------------------------------------- //

void handleAttach()
{
    rKey = GetAsyncKeyState(0x52) < 0;

    if (rKey)
    {
        if (!dbAttach)
        {
            dbAttach = true;
            Attach();
        }
    }
    else
    { dbAttach = false; }
}

void Attach()
{
    try
    {
        ram = new Swed("HelloNeighborReborn-Win64-Shipping");
        game = ram.GetModuleBase("HelloNeighborReborn-Win64-Shipping.exe");
        //physX = ram.GetModuleBase("PhysX3PROFILE_x64.dll");
        attached = true;
        print("[ Game process found! ]", ConsoleColor.Green);


        // PLAYER
        posX = ram.ReadPointer(game, poiPlayer, poiPlayer2, poiPlayer3) + poiPosX;
        posY = ram.ReadPointer(game, poiPlayer, poiPlayer2, poiPlayer3) + poiPosY;
        posZ = ram.ReadPointer(game, poiPlayer, poiPlayer2, poiPlayer3) + poiPosZ;
        forceX = ram.ReadPointer(game, poiPlayer, poiPlayer2, poiPlayer3) + poiForceX;
        forceY = ram.ReadPointer(game, poiPlayer, poiPlayer2, poiPlayer3) + poiForceY;
        forceZ = ram.ReadPointer(game, poiPlayer, poiPlayer2, poiPlayer3) + poiForceZ;
        speed = ram.ReadPointer(game, poiScripts, poiSpeed, poiSpeed2) + poiSpeed3;

        engineMovement = game + 0x3CA1F8; // original = 0F 29 B3 20 01 00 00
        engineRotation = game + 0x3CA1EE; // original = 0F 29 B3 20 01 00 00

        // CAMERA
        camX = ram.ReadPointer(game, poiCamera, poiCamProperties) + poiCamX; //estos son solo pointers, no los valores. esta asi para que luego se pueda writear
        camY = ram.ReadPointer(game, poiCamera, poiCamProperties) + poiCamY;
        camZ = ram.ReadPointer(game, poiCamera, poiCamProperties) + poiCamZ;
        camFov = ram.ReadPointer(game, poiCamera, poiCamProperties, poiCamDisplay) + poiCamFov;

        // NEIGHBOR POS
        nPosX = ram.ReadPointer(game, poiScripts, poiNeighbor, poiNeighborPos) + poiNeighborX;
        nPosY = ram.ReadPointer(game, poiScripts, poiNeighbor, poiNeighborPos) + poiNeighborY;
        nPosZ = ram.ReadPointer(game, poiScripts, poiNeighbor, poiNeighborPos) + poiNeighborZ;
        nWide = ram.ReadPointer(game, poiScripts, poiNeighbor, poiNeighborPos) + poiNeighborWIDE;

        // POST PROCESSING COLORS
        cRed = ram.ReadPointer(game, poiPostprocess, poiPostprocess2, poiColors) + poiRed;
        cGreen = ram.ReadPointer(game, poiPostprocess, poiPostprocess2, poiColors) + poiGreen;
        cBlue = ram.ReadPointer(game, poiPostprocess, poiPostprocess2, poiColors) + poiBlue;
    }
    catch
    {
        print("[ Game process not found! R to reload ]" , ConsoleColor.Red);
        attached = false;
    }
}

// -------------------------------------------- DECLARE VARIABLES -------------------------------------------- //

void Variables()
{
    // KEYS
    spaceKey = GetAsyncKeyState(0x20) < 0;
    shiftKey = GetAsyncKeyState(0xA0) < 0;
    controlKey = GetAsyncKeyState(0xA2) < 0;
    vKey = GetAsyncKeyState(0x56) < 0;
    wKey = GetAsyncKeyState(0x57) < 0;
    aKey = GetAsyncKeyState(0x41) < 0;
    sKey = GetAsyncKeyState(0x53) < 0;
    dKey = GetAsyncKeyState(0x44) < 0;
    xKey = GetAsyncKeyState(0x58) < 0;
    zKey = GetAsyncKeyState(0x5A) < 0;
    tKey = GetAsyncKeyState(0x54) < 0;


    // PLAYER
    currPosX = ram.ReadFloat(posX);
    currPosY = ram.ReadFloat(posY);
    currPosZ = ram.ReadFloat(posZ);

    // FOV
    finalFov = renderer.basefov;

    // ITEMS
    //crowbarPos.Z = ram.ReadPointer(physX, physics, items, it_crowbar) + it_crowbarY;
    //print(crowbarPos);
}

// ------------------------------------------ NO ROTATION ----------------------------------------- //

void NoRotationMode()
{
    if (renderer.norotation)
    ram.WriteBytes(engineRotation, "90 90 90 90 90 90 90"); // DISABLES engineRotation
    else
    ram.WriteBytes(engineRotation, "0F 29 A3 10 01 00 00"); // ENABLES engineRotation
}


// ------------------------------------------- WIDE MODE ----------------------------------------- //

void WideMode()
{
    if (renderer.wide)
    {
        ram.WriteFloat(nWide, renderer.wideamm);
    }
}

// ---------------------------------------- TP TO NEIGHBOR --------------------------------------- //

void ToNeighbor()
{

    if (renderer.toneighbor)
    {
        ram.WriteFloat(posX, ram.ReadFloat(nPosX) + 50);
        ram.WriteFloat(posY, ram.ReadFloat(nPosY) + 100);
        ram.WriteFloat(posZ, ram.ReadFloat(nPosZ) + 50);
        ram.WriteFloat(forceY, 0);
        ram.WriteFloat(forceY, 0);
        ram.WriteFloat(forceX, 0);
    }
}

// -------------------------------------------- SPEED -------------------------------------------- //

void Speed()
{
    if (!renderer.fly & renderer.ovspeed)
    {
        if (!controlKey)
        {
            ram.WriteFloat(speed, renderer.walkspeed);
        }
        if (shiftKey & !aKey & !dKey)
        {
            ram.WriteFloat(speed, renderer.runspeed);
        }
    }
}

// --------------------------------------------- FLY --------------------------------------------- //

void Fly()
{
    // NOCLIP ON KEY
    if (vKey)
    {
        if (dbFly == false)
        {
            dbFly = true;
            renderer.fly = !renderer.fly;
            lastY = ram.ReadFloat(posY);
        }
    }
    else
    { dbFly = false; }

    if (zKey) { renderer.flySpeed *= 1.05f; }
    if (xKey) { renderer.flySpeed /= 1.05f; }

    // ACTUALLY FLY

    vAngles.X = ram.ReadFloat(camX);
    vAngles.Y = ram.ReadFloat(camY);
    vAngles.Z = ram.ReadFloat(camZ);

    //calcular nuevas posiciones

    float addX = (float)(renderer.flySpeed * Math.Sin(vAngles.Y * degreeToRadian - anglesOffset));
    float addZ = (float)(renderer.flySpeed * Math.Cos(vAngles.Y * degreeToRadian - anglesOffset));

    if (renderer.fly)
    {
        ram.WriteFloat(posY, lastY);
        if (wKey)
        {
            ram.WriteFloat(posX, currPosX - addX);
            ram.WriteFloat(posZ, currPosZ + addZ);
        }

        if (sKey)
        {
            ram.WriteFloat(posX, currPosX + addX);
            ram.WriteFloat(posZ, currPosZ - addZ);
        }

        // UP AND DOWN
        if (spaceKey)
        {
            ram.WriteFloat(posY, ram.ReadFloat(posY) + renderer.flySpeed); // UP ON SPACE
            lastY = ram.ReadFloat(posY);
        }
        if (controlKey)
        {
            ram.WriteFloat(posY, ram.ReadFloat(posY) - renderer.flySpeed); // DOWN ON CONTROL
            lastY = ram.ReadFloat(posY);
        }
        ram.WriteFloat(forceX, 0);
        ram.WriteFloat(forceY, 0);
        ram.WriteFloat(forceZ, 0);
        if (renderer.alter)
        { ram.WriteBytes(engineMovement, "90 90 90 90 90 90 90"); } // DISABLES engineMovement

    }
    else
    {
        ram.WriteBytes(engineMovement, "0F 29 B3 20 01 00 00"); // ENABLES engineMovement
    }
}



// -------------------------------------- NEIGHBOR AMBIENCE -------------------------------------- //
void Ambience()
{
    float xdist = ram.ReadFloat(posX) - ram.ReadFloat(nPosX);
    float ydist = ram.ReadFloat(posY) - ram.ReadFloat(nPosY);
    float zdist = ram.ReadFloat(posZ) - ram.ReadFloat(nPosZ);
    distance = (float)Math.Sqrt(Math.Pow(xdist, 2) + Math.Pow(ydist, 2) + Math.Pow(zdist, 2));

    float red = ram.ReadFloat(cRed);
    float green = ram.ReadFloat(cGreen);
    float blue = ram.ReadFloat(cBlue);

    if (distance > renderer.ambdist + 200)
    {
        ogColor = new Vector3(red, green, blue);
    }
    if (ogColor.X != 0)
    {
        ram.WriteFloat(cRed, ogColor.X);
        ram.WriteFloat(cGreen, ogColor.Y);
        ram.WriteFloat(cBlue, ogColor.Z);
    }
    normdist = (renderer.ambdist - distance) / renderer.ambdist;
    if (renderer.ambience)
    {
        if (distance < renderer.ambdist)
        {
            ram.WriteFloat(cRed, ogColor.X - ogColor.X * normdist * 1);
            ram.WriteFloat(cGreen, ogColor.Y - ogColor.Y * normdist * 12);
            ram.WriteFloat(cBlue, ogColor.Z - ogColor.Z * normdist * 12);

            ram.WriteFloat(camX, ram.ReadFloat(camX) + (float)Math.Cos(time * 10) * normdist);
            ram.WriteFloat(camY, ram.ReadFloat(camY) + (float)Math.Sin(time * 10) * normdist);

            finalFov = finalFov - 20 * (float)Math.Sqrt(normdist);

            if (distance < renderer.ambdist - 400 & renderer.neon)
            {
                ram.WriteFloat(cRed, ogColor.X - ogColor.X * normdist * 100);
            }
        }
    }
}



// ----------------------------------------- VIEW BOBBING ---------------------------------------- //
void Bobbing()
{
    if (renderer.bobbing)
    {
        Vector3 camRot = new Vector3(ram.ReadFloat(camX), ram.ReadFloat(camY), ram.ReadFloat(camZ));

        Vector2 axis = new Vector2(0, 0);
        axis.Y = axis.Y + (float)Convert.ToInt32(wKey);
        axis.Y = axis.Y - (float)Convert.ToInt32(sKey);
        axis.X = axis.X + (float)Convert.ToInt32(dKey);
        axis.X = axis.X - (float)Convert.ToInt32(aKey);

        float roll = Lerp(vAngles.Z, -2 * axis.X, 0.1f);
        //float pitch = Lerp(vAngles.X, -10 * axis.Y, 0.1f);

        print(roll);
        //print(pitch);

        //ram.WriteFloat(camX, pitch);
        if (roll < 0)
        {
            float or = roll;
            roll = 360 - or;
        }
        ram.WriteFloat(camZ, roll);
    }
}




// ----------------------------------------- DYNAMIC FOV ----------------------------------------- //
void DynamicFov()
{

    if (renderer.dynfov)
    {
        float sp = (ram.ReadFloat(speed) / 600) - 1;

        finalFov = finalFov * (1 + sp / 7);
    }
}

// ------------------------------------------ FOV CHANGE ----------------------------------------- //

void UpdateFov()
{
    float cFov = Lerp(ram.ReadFloat(camFov), finalFov, 0.1f);
    if (cFov < 1){cFov = 3;}
    ram.WriteFloat(camFov, cFov);
}


// -------------------------------------------- FREEZE -------------------------------------------- //
void Freeze()
{
    if (tKey)
    {
        if (dbFreeze == false)
        {
            dbFreeze = true;
            renderer.freezeNeighbor = !renderer.freezeNeighbor;
            oldNPosX = ram.ReadFloat(nPosX);
            oldNPosY = ram.ReadFloat(nPosY);
            oldNPosZ = ram.ReadFloat(nPosZ);

            if (renderer.freezePos.X != 0 & renderer.freezePos.Y != 0 & renderer.freezePos.Z != 0)
            {
                oldNPosX = renderer.freezePos.X;
                oldNPosY = renderer.freezePos.Y;
                oldNPosZ = renderer.freezePos.Z;
            }

            if (renderer.freezeNeighbor)
            {
                print("- Neighbor freezed at:");
                print("      X: " + oldNPosX + " Y: " + oldNPosY + " Z: " + oldNPosZ, ConsoleColor.DarkGray);
                if (renderer.freezeToPlayer)
                {
                    print("      Relative to Player", ConsoleColor.DarkGray);
                }
            }
            else
            { print("- Neighbor Unfreezed"); }
        }
    }
    else
    { dbFreeze = false; }
    float neiCamX = (float)(Math.Cos(vAngles.Y * degreeToRadian - anglesOffset));
    float neiCamY = (float)(Math.Sin(vAngles.Y * degreeToRadian - anglesOffset));
    float neiCamZ = (float)(Math.Sin(vAngles.X * degreeToRadian));

    if (renderer.freezeNeighbor)
    {
        if (renderer.freezeToPlayer)
        {
            oldNPosX = currPosX - neiCamY * 300;
            oldNPosY = currPosY + 50 + neiCamZ * 300;
            oldNPosZ = currPosZ + neiCamX * 300;
        }
        ram.WriteFloat(nPosX, oldNPosX);
        ram.WriteFloat(nPosY, oldNPosY);
        ram.WriteFloat(nPosZ, oldNPosZ);
    }
}

// --------------------------------------------- PUSH --------------------------------------------- //
void Push()
{
    float xdist = ram.ReadFloat(posX) - ram.ReadFloat(nPosX);
    float ydist = ram.ReadFloat(posY) - ram.ReadFloat(nPosY);
    float zdist = ram.ReadFloat(posZ) - ram.ReadFloat(nPosZ);

    float localPushDist = (float)Math.Sqrt(Math.Pow(xdist, 2) + Math.Pow(ydist, 2) + Math.Pow(zdist, 2));

    if (renderer.push & localPushDist <= renderer.pushDistance & ydist < 100)
    {
        ram.WriteFloat(nPosX, ram.ReadFloat(nPosX) - xdist*0.01f);
        ram.WriteFloat(nPosZ, ram.ReadFloat(nPosZ) - zdist*0.01f);
    }    
}

// --------------------------------------------- MISC --------------------------------------------- //


float Lerp(float firstFloat, float secondFloat, float by)
{
    return firstFloat * (1 - by) + secondFloat * by;
} //admito que esta funcion la robe de https://stackoverflow.com/questions/33044848/c-sharp-lerping-from-position-to-position XDD

float Rad(float angle)
{ return (float)(Math.PI / 180) * angle; }

void print(object text, ConsoleColor fColor = ConsoleColor.DarkGreen)
{
    Console.ForegroundColor = fColor;
    Console.WriteLine(text);
}

[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int key_Gravity);




/*EXPLICACION RAPIDA
nint player = ram.ReadPointer(game, poiPlayer, poiPlayer2, poiPlayer3) + poiPosY;
                    |          |             |           |           |           |
       lee la direccion      base          child       child      child       lo que tiene que leer

en lugar de hacer ReadFloat(PLAYER, OFFSET), se le suma el offset poiPosY al "player", para solo tener que hacer ReadFloat(PLAYER), ya que el "+" fuciona para
predecir lo que vas a leer
*/
