namespace unrealTurtlesLib
open UnrealEngine.Framework
open System.Drawing
open System

module Advent =
    type Point = int * int * int
    type Points = Set<Point> 

    let add ((x1,y1,z1): Point) ((x2,y2,z2) : Point) : Point = (x1+x2, y1+y2, z1+z2)

    let neighbors (p:Point) : Points =  
        let diff = [-1;0;1]; 
        seq { 
            for x in diff do
                for y in diff do
                    for z in diff do
                            if (x,y,z) <> (0,0,0) then 
                                yield (add p (x,y,z))
        } |> Set.ofSeq

    // returns a set of possible cells that might be alive
    let findCandidates (s:Points) : Points = 
        s |>  Set.map (neighbors) |> Set.unionMany

    let countAliveNeighbors (p:Point) (s: Points): int =
        let n = neighbors p
        let aliveNeighbors = n |> Set.intersect s
        aliveNeighbors |> Seq.length

    let nextGeneration (s: Points) : Points = 
        let candidates = findCandidates s
        candidates |> Set.filter ( fun c -> let isAlive = Set.contains c s  
                                            let aliveNeighbors = countAliveNeighbors c s 
                                            if (isAlive && (aliveNeighbors = 2|| aliveNeighbors = 3)) then true
                                            else if (not isAlive && (aliveNeighbors = 3)) then true
                                            else false )

    let inputData : Points =
        let mutable i = 0
        let mutable j = 0
        let points = seq {
            for line in IO.File.ReadAllLines("Managed\Build\input.txt") do
                j <- 0
                i <- i + 1 
                for char in line do
                    j <- j + 1
                    if (char = '#') then yield (i,j,0)
        }
        points |> Set.ofSeq
        
module mainModule = 
    open Advent
    let foo = "hei" 
    let actor: Actor= Actor("MainActor")
    let iSMC = new InstancedStaticMeshComponent(actor, setAsRoot=true)
    let material = Material.Load("/Game/StarterContent/Materials/M_Metal_Gold.M_Metal_Gold")
    let rotationSpeed = 2.5f
    let sc: SceneComponent = SceneComponent(actor)

    let mutable currentGen = inputData 
    let mutable transforms : Transform [] = Array.empty 
    let mutable instances : int [] = Array.empty 

open mainModule
open System.Diagnostics
open Advent

type Main () =
    static member OnWorldBegin (): unit = 
        Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink, (currentGen|> Set.count).ToString())
    static  member OnWorldPostBegin() :unit =
        Debug.AddOnScreenMessage(-1, 10.0f, Color.Green, foo)
        World.GetFirstPlayerController().SetViewTarget(World.GetActor<Camera>("MainCamera"))
        iSMC.SetStaticMesh(StaticMesh.Sphere) |> ignore
        iSMC.SetMaterial(0, material)
        let color = LinearColor.White
        iSMC.CreateAndSetMaterialInstanceDynamic(0).SetVectorParameterValue("Color", &color)
        ()

    static member OnWorldPrePhysicsTick(deltaTime: single) =
        Debug.AddOnScreenMessage(1, 1.0f, Color.SkyBlue, sprintf "Delta number: %A" deltaTime);
        currentGen <- (nextGeneration currentGen)
        for inst in instances do 
            iSMC.RemoveInstance(inst)

        transforms <- Array.zeroCreate<Transform> (currentGen |> Seq.length) 
        let instances = Array.zeroCreate<int> (transforms.Length)
        let mutable i = 0
        for cell in currentGen do
            let (x,y,z) = cell
            let xf = single x * 100.0f
            let yf = single y * 100.0f
            let zf = single z * 100.0f
            //let tf = single t * 100.0f
            let relLoc = Numerics.Vector3(xf,yf,zf)
            sc.SetRelativeLocation(&relLoc)

            sc.GetTransform(&(transforms.[i]))
            let instance = iSMC.AddInstance(&(transforms.[i]))
            instances.[i] <- instance
            i <- i + 1
        
            //sc.SetWorldTransform(&(transforms.[i]))
           // sc.GetTransform(&(transforms.[i]))
        iSMC.BatchUpdateInstanceTransforms(0, transforms, markRenderStateDirty = true) |> ignore
        ()