namespace unrealTurtlesLib
open UnrealEngine.Framework
open System.Drawing
open System

module Say =
    let hello name =
        printfn "Hello %s" name

module mainModule = 
    let foo = "hei"
    let actor: Actor= Actor("MainActor")
    let transforms : Transform [] = Array.zeroCreate<Transform> 200 
    let iSMC = new InstancedStaticMeshComponent(actor, setAsRoot=true)
    let material = Material.Load("/Game/Tests/BasicMaterial")
    let rotationSpeed = 2.5f
    let sc: SceneComponent = SceneComponent(actor)
    let maxCubes = 200 

open mainModule
open System.Diagnostics

type Main () =
    static member OnWorldBegin (): unit = 
        Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink, foo)
    static  member OnWorldPostBegin() :unit =
        Debug.AddOnScreenMessage(-1, 10.0f, Color.Green, "Onplay.... !");
        World.GetFirstPlayerController().SetViewTarget(World.GetActor<Camera>("MainCamera"))
        iSMC.SetStaticMesh(StaticMesh.Cube) |> ignore
        iSMC.SetMaterial(0, material)
        let color = LinearColor.White
        iSMC.CreateAndSetMaterialInstanceDynamic(0).SetVectorParameterValue("Color", &color)
        for i in 0 .. maxCubes - 1 do
            let iF = single i
            let relLoc = Numerics.Vector3(150.0f * iF, 10.0f * iF, 10.0f * iF)
            sc.SetRelativeLocation(&relLoc) // https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/byrefs

            let rot = Maths.CreateFromYawPitchRoll(5.0f * iF, 0.0f, 0.0f) 
            sc.SetRelativeRotation(&rot)

            let offset = Numerics.Vector3(15.0f, 20.0f * iF, 25.0f * iF)
            sc.AddLocalOffset(&offset)
            //Debugger.Launch()

            sc.GetTransform(&(transforms.[i]))
            iSMC.AddInstance(&(transforms.[i])) |> ignore
        ()

    static member OnWorldPrePhysicsTick(deltaTime: single) =
        Debug.AddOnScreenMessage(1, 1.0f, Color.SkyBlue, sprintf "Delta number: %A" deltaTime);
        let dR = Maths.CreateFromYawPitchRoll(rotationSpeed * deltaTime, rotationSpeed * deltaTime, rotationSpeed * deltaTime)
        
        for i in 0 .. maxCubes - 1 do
            sc.SetWorldTransform(&(transforms.[i]))
            sc.AddLocalRotation(&dR)
            sc.GetTransform(&(transforms.[i]))
        iSMC.BatchUpdateInstanceTransforms(0, transforms, markRenderStateDirty = true) |> ignore
        ()