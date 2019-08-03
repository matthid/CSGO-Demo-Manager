
#r @"C:\Users\matth\.nuget\packages\newtonsoft.json\12.0.2\lib\net45\Newtonsoft.Json.dll"
#r @"C:\Users\matth\.nuget\packages\thoth.json.net\3.5.1\lib\net46\Thoth.Json.Net.dll";;

#load "Thoth_Utils.fs";;

open Thoth.Json.Net
open System.Reflection
open System.Reflection.Emit
open FSharp.Reflection

//let item = 123,13536,"tesFDt"
//let t = item.GetType()
//let tup = item :> obj :?> System.Runtime.CompilerServices.ITuple
type TestDU =
    | FirstCase
    | SecondCase of int * string
    | ThirdCase of int * TestDU
type TestRecord = {
    Name : string
    KillAward : int
}

let generateGetter<'t> (prop:System.Reflection.PropertyInfo) =
    let funcType = typeof<System.Func<obj, 't>>
    let parentType = prop.DeclaringType
    let getMethod = prop.GetGetMethod()
    
    try
        // https://stackoverflow.com/questions/51028886/create-delegate-of-getter-with-changed-return-type
        let dynMethod = new DynamicMethod(System.String.Format("Dynamic_Get_{0}_{1}", parentType.Name, prop.Name), typeof<'t>, [| typeof<obj> |], parentType.Module);
        let ilGen = dynMethod.GetILGenerator()
            
        ilGen.Emit(OpCodes.Ldarg_0) // what if parentType is a Value Type?
        ilGen.Emit(OpCodes.Callvirt, prop.GetGetMethod())
        if prop.PropertyType.IsValueType && typeof<'t> <> prop.PropertyType then
            ilGen.Emit(OpCodes.Box, prop.PropertyType)
        else
            ()
        ilGen.Emit(OpCodes.Ret)   
        dynMethod.CreateDelegate(funcType) :?> System.Func<obj, 't>
        //System.Delegate.CreateDelegate(funcType, null, getMethod) :?> System.Func<obj, 't>
    with e ->
        let args = System.String.Join(",", getMethod.GetParameters() |> Seq.map (fun p -> sprintf "%s %s" p.ParameterType.FullName p.Name))
        let signature = sprintf "%s %s.%s(%s)" getMethod.ReturnType.FullName getMethod.DeclaringType.FullName getMethod.Name args
        printfn "ERROR for generateGetter<%s> on CreateDelegate(%s, null, {%s}): %O" typeof<'t>.FullName funcType.FullName signature e
        System.Func<obj, 't>(fun (v:obj) ->
            prop.GetGetMethod().Invoke(v, [||]) :?> 't)

let item1 = FirstCase
let item2 = SecondCase (2, "test")
let item3 = ThirdCase (2, FirstCase)
let t1 = item1.GetType()
let t2 = item2.GetType()
let t3 = item3.GetType()
let b = typeof<TestDU>

let itemRec = { Name = "Test"; KillAward = 3 }
let tRec = itemRec.GetType()

let fields = FSharpType.GetRecordFields(tRec, allowAccessToPrivateRepresentation=true)
fields
|> Seq.map (fun f -> f, generateGetter f)
|> Seq.iter (fun (f, acc) -> printfn "%s: %O" f.Name (acc.Invoke itemRec))

(*
t.GetFields(BindingFlags.NonPublic ||| BindingFlags.Instance ||| BindingFlags.FlattenHierarchy ||| BindingFlags.Static ||| BindingFlags.Public)
t.GetProperties(BindingFlags.NonPublic ||| BindingFlags.Instance ||| BindingFlags.FlattenHierarchy ||| BindingFlags.Static ||| BindingFlags.Public)
t.GetMethods(BindingFlags.NonPublic ||| BindingFlags.Instance ||| BindingFlags.FlattenHierarchy ||| BindingFlags.Static ||| BindingFlags.Public)

*)