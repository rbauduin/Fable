﻿module Fable.Tests.TypeTests

open System
open Util.Testing

type ITest = interface end
type ITest2 = interface end

type TestType(s: string) =
    member __.Value = s
    interface ITest

type TestType2(s: string) =
    member __.Value = s
    interface ITest

type TestType3() =
    member __.Value = "Hi"
    interface ITest

type TestType4() =
    inherit TestType3()
    member __.Value2 = "Bye"
    interface ITest2

type TestType5(greeting: string) =
    member __.Value = greeting
    member __.Overload(x) = x + x
    member __.Overload(x, y) = x + y

type TestType8(?greeting) =
    member __.Value = defaultArg greeting "Hi"

type TestType9() =
    inherit TestType8()
    let foo = TestType8("Hello")
    member __.Greet(name) = foo.Value + " " + name

type RenderState =
    { Now : int
      Players : Map<int, string>
      Map : string }

type T4 = TestType4

type TestType6(x: int) =
    let mutable i = x
    member val Value1 = i with get, set
    member __.Value2 = i + i
    member __.Value3 with get() = i * i and set(v) = i <- v

type TestType7(a1, a2, a3) =
    let arr = [|a1; a2; a3|]
    member __.Value with get(i) = arr.[i] and set(i) (v) = arr.[i] <- v

type A  = { thing: int } with
    member x.show() = string x.thing
    static member show (x: A) = "Static: " + (string x.thing)

type B  = { label: string } with
    member x.show() = x.label
    static member show (x: B) = "Static: " + x.label

let inline show< ^T when ^T : (member show : unit -> string)> (x:^T) : string =
   (^T : (member show : unit -> string) (x))

let inline showStatic< ^T when ^T : (static member show : ^T -> string)> (x:^T) : string =
   (^T : (static member show : ^T -> string) (x))

[<AllowNullLiteral>]
type Serializable(?i: int) =
    let mutable deserialized = false
    let mutable publicValue = 1
    let mutable privateValue = defaultArg i 0
    member x.PublicValue
        with get() = publicValue
        and set(i) = deserialized <- true; publicValue <- i
    override x.ToString() =
        sprintf "Public: %i - Private: %i - Deserialized: %b"
                publicValue privateValue deserialized

type SecondaryCons(x: int) =
    new () = SecondaryCons(5)
    member __.Value = x

type MultipleCons(x: int, y: int) =
    new () = MultipleCons(2,3)
    new (x:int) = MultipleCons(x,4)
    member __.Value = x + y

[<AbstractClass>]
type AbstractClassWithDefaults () =
    abstract MethodWithDefault : unit -> string
    default x.MethodWithDefault () = "Hello "

    abstract MustImplement: unit -> string

    member x.CallMethodWithDefault () =
        x.MethodWithDefault() + x.MustImplement()

type ConcreteClass () =
    inherit AbstractClassWithDefaults()
    override x.MustImplement () = "World!!"

type ConcreteClass2 () =
    inherit AbstractClassWithDefaults()
    override x.MethodWithDefault () = "Hi "
    override x.MustImplement () = "World!!"

type ISomeInterface =
    abstract OnlyGetProp: int with get
    abstract OnlyProp: int
    abstract Sender : int with get, set

type XISomeInterface () =
    let mutable i = 0
    interface ISomeInterface with
        member x.OnlyGetProp
            with get () = 0
        member x.OnlyProp = 3
        member x.Sender
            with get () = i
            and set i' = i <- i'

type IFoo =
    abstract Foo: unit -> string
    abstract Bar: string
    abstract MySetter: int with get, set

let mangleFoo(x: IFoo) = x.Foo()

type FooImplementor(i: int) =
    let mutable mut1 = 0
    let mutable mut2 = 5
    new () = FooImplementor(1)

    member x.Foo() = String.replicate i "foo"
    member x.Bar = "he"
    member x.MySetter with get() = mut1 and set(v) = mut1 <- v + 2

    interface IFoo with
        member x.Foo() = x.Foo() + "bar"
        member x.Bar = x.Bar + "ho"
        member x.MySetter with get() = mut1 + mut2 and set(v) = mut2 <- v + 3

type FooImplementorChild() =
    inherit FooImplementor(3)

[<AbstractClass>]
type AbstractFoo() =
    abstract member Foo: unit -> string
    interface IFoo with
        member this.Foo() = this.Foo() + "FOO"
        member x.Bar = ""
        member x.MySetter with get() = 0 and set(v) = ()

type ChildFoo() =
    inherit AbstractFoo()
    override this.Foo() = "BAR"

type BaseClass () =
    abstract member Init: unit -> int
    default self.Init () = 5

type ExtendedClass () =
    inherit BaseClass ()

    override self.Init() =
        base.Init() + 2

type Employee = { name: string; age: float; location: Location }
and Location = { name: string; mutable employees: Employee list }

[<Struct>]
type ValueType<'T> =
    new (f) = { foo = f }
    val foo : 'T
    member x.Value = x.foo

[<Struct>]
type ValueType2(i: int, j: int) =
    member x.Value = i + j

type Point2D =
   struct
      val X: float
      val Y: float
      new(xy: float) = { X = xy; Y = xy }
   end

exception MyEx of int*string

type MyEx2(f: float) =
  inherit Exception(sprintf "Code: %i" (int f))
  member __.Code = f

let tests =
  testList "Types" [
    testCase "Types can instantiate their parent in the constructor" <| fun () ->
        let t = TestType9()
        t.Greet("Maxime") |> equal "Hello Maxime"

    testCase "Type testing" <| fun () ->
        let x = TestType "test" :> obj
        let y = TestType2 "test" :> obj
        x :? TestType |> equal true
        x :? TestType2 |> equal false
        y :? TestType |> equal false
        y :? TestType2 |> equal true

    testCase "Type testing in pattern matching" <| fun () ->
        let x = TestType "test" :> obj
        match x with
        | :? TestType as x -> x.Value
        | _ -> "FAIL"
        |> equal "test"
        match x with
        | :? TestType2 as x -> x.Value
        | _ -> "FAIL"
        |> equal "FAIL"

    // TODO: Should we make interface testing work in Fable 2?
    // testCase "Children inherits parent interfaces" <| fun () ->
    //     let t4 = TestType4() |> box
    //     t4 :? ITest |> equal true

    // testCase "Interface testing" <| fun () ->
    //     let x = Union1 "test" :> obj
    //     let y = Union2 "test" :> obj
    //     x :? ITest |> equal true
    //     x :? ITest2 |> equal false
    //     y :? ITest |> equal true
    //     y :? ITest2 |> equal false

    // testCase "Interface testing in pattern matching" <| fun () ->
    //     let x = Union2 "test" :> obj
    //     match x with | :? ITest -> true | _ -> false
    //     |> equal true
    //     match x with | :? ITest2 -> true | _ -> false
    //     |> equal false

    testCase "Type testing with JS primitive types works" <| fun () ->
        let test (o: obj) =
            match o with
            | :? string -> "string"
            | :? float -> "number"
            | :? bool -> "boolean"
            | :? unit -> "null/undefined"
            | :? (unit->unit) -> "function"
            | :? System.Text.RegularExpressions.Regex -> "RegExp"
            | :? (int[]) | :? (string[]) -> "Array"
            | _ -> "unknown"
        "A" :> obj |> test |> equal "string"
        3. :> obj |> test |> equal "number"
        false :> obj |> test |> equal "boolean"
        () :> obj |> test |> equal "null/undefined"
        (fun()->()) :> obj |> test |> equal "function"
        System.Text.RegularExpressions.Regex(".") :> obj |> test |> equal "RegExp"
        [|"A"|] :> obj |> test |> equal "Array"
        [|1;2|] :> obj |> test |> equal "Array"

    testCase "Type test with Date" <| fun () ->
        let isDate (x: obj) =
            match x with
            | :? DateTime -> true
            | _ -> false
        DateTime.Now |> box |> isDate |> equal true
        box 5 |> isDate |> equal false

    testCase "Type test with Long" <| fun () ->
        let isLong (x: obj) =
            match x with
            | :? int64 -> true
            | _ -> false
        box 5L |> isLong |> equal true
        box 50 |> isLong |> equal false

    testCase "Type test with BigInt" <| fun () ->
        let isBigInd (x: obj) =
            match x with
            | :? bigint -> true
            | _ -> false
        box 5I |> isBigInd |> equal true
        box 50 |> isBigInd |> equal false

    testCase "Property names don't clash with built-in JS objects" <| fun () -> // See #168
        let gameState = {
            Now = 1
            Map = "dungeon"
            Players = Map.empty
        }
        gameState.Players.ContainsKey(1) |> equal false

    testCase "Overloads work" <| fun () ->
        let t = TestType5("")
        t.Overload(2) |> equal 4
        t.Overload(2, 3) |> equal 5

    testCase "Type abbreviation works" <| fun () ->
        let t = T4()
        t.Value2 |> equal "Bye"

    testCase "Getter and Setter work" <| fun () ->
        let t = TestType6(5)
        t.Value1 |> equal 5
        t.Value2 |> equal 10
        t.Value3 |> equal 25
        t.Value3 <- 10
        t.Value1 |> equal 5
        t.Value2 |> equal 20
        t.Value3 |> equal 100
        t.Value1 <- 20
        t.Value1 |> equal 20
        t.Value2 |> equal 20
        t.Value3 |> equal 100

    testCase "Getter and Setter with indexer work" <| fun () ->
        let t = TestType7(1, 2, 3)
        t.Value(1) |> equal 2
        t.Value(2) |> equal 3
        t.Value(1) <- 5
        t.Value(1) |> equal 5
        t.Value(2) |> equal 3

    testCase "Statically resolved instance calls work" <| fun () ->
        let a = { thing = 5 }
        let b = { label = "five" }
        show a |> equal "5"
        show b |> equal "five"

    testCase "Statically resolved static calls work" <| fun () ->
        let a = { thing = 5 }
        let b = { label = "five" }
        showStatic a |> equal "Static: 5"
        showStatic b |> equal "Static: five"

    testCase "Guid.NewGuid works" <| fun () ->
        let g1 = Guid.NewGuid()
        let g2 = Guid.NewGuid()
        g1 = g2 |> equal false
        let s1 = string g1
        equal 36 s1.Length
        Text.RegularExpressions.Regex.IsMatch(
            s1, "^[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}$")
        |> equal true
        let g3 = Guid.Parse s1
        g1 = g3 |> equal true

    testCase "Guid.Empty works" <| fun () ->
        let g1 = Guid.Empty
        string g1 |> equal "00000000-0000-0000-0000-000000000000"

    testCase "lazy works" <| fun () ->
        let mutable snitch = 0
        let lazyVal =
            lazy
                snitch <- snitch + 1
                5
        equal 0 snitch
        equal 5 lazyVal.Value
        equal 1 snitch
        lazyVal.Force() |> equal 5
        equal 1 snitch

    testCase "Lazy.CreateFromValue works" <| fun () ->
        let mutable snitch = 0
        let lazyVal =
            Lazy<_>.CreateFromValue(
                snitch <- snitch + 1
                5)
        equal 1 snitch
        equal 5 lazyVal.Value
        equal 1 snitch

    testCase "lazy.IsValueCreated works" <| fun () ->
        let mutable snitch = 0
        let lazyVal =
            Lazy<_>.Create(fun () ->
                snitch <- snitch + 1
                5)
        equal 0 snitch
        equal false lazyVal.IsValueCreated
        equal 5 lazyVal.Value
        equal true lazyVal.IsValueCreated
        lazyVal.Force() |> equal 5
        equal true lazyVal.IsValueCreated

    testCase "Lazy constructor works" <| fun () ->
        let items = Lazy<string list>(fun () -> ["a";"b";"c"])
        let search e = items.Value |> List.tryFind (fun m -> m = e)
        search "b" |> equal (Some "b")
        search "d" |> equal None

    // testCase "Classes can be JSON serialized forth and back" <| fun () ->
    //     let x = Serializable(5)
    //     #if FABLE_COMPILER
    //     let json = Fable.Core.JsInterop.toJson x
    //     let x2 = Fable.Core.JsInterop.ofJson<Serializable> json
    //     string x |> equal "Public: 1 - Private: 5 - Deserialized: false"
    //     string x2 |> equal "Public: 1 - Private: 0 - Deserialized: true"
    //     let x2 = Fable.Core.JsInterop.ofJsonAsType json (x.GetType()) :?> Serializable
    //     string x |> equal "Public: 1 - Private: 5 - Deserialized: false"
    //     string x2 |> equal "Public: 1 - Private: 0 - Deserialized: true"
    //     let json = Fable.Core.JsInterop.toJsonWithTypeInfo x
    //     let x2 = Fable.Core.JsInterop.ofJsonWithTypeInfo<Serializable> json
    //     #else
    //     let json = Newtonsoft.Json.JsonConvert.SerializeObject x
    //     let x2 = Newtonsoft.Json.JsonConvert.DeserializeObject<Serializable> json
    //     #endif
    //     string x |> equal "Public: 1 - Private: 5 - Deserialized: false"
    //     string x2 |> equal "Public: 1 - Private: 0 - Deserialized: true"

    // testCase "Null values can be JSON serialized forth and back" <| fun () ->
    //     let x: Serializable = null
    //     #if FABLE_COMPILER
    //     let json = Fable.Core.JsInterop.toJson x
    //     let x2 = Fable.Core.JsInterop.ofJsonAsType json (typedefof<Serializable>)
    //     equal x2 null
    //     let json = Fable.Core.JsInterop.toJson x
    //     let x2 = Fable.Core.JsInterop.ofJson<Serializable> json
    //     equal x2 null
    //     let json = Fable.Core.JsInterop.toJsonWithTypeInfo x
    //     let x2 = Fable.Core.JsInterop.ofJsonWithTypeInfo<Serializable> json
    //     #else
    //     let json = Newtonsoft.Json.JsonConvert.SerializeObject x
    //     let x2 = Newtonsoft.Json.JsonConvert.DeserializeObject<Serializable> json
    //     #endif
    //     equal x2 null

    // testCase "Classes serialized with Json.NET can be deserialized" <| fun () ->
    //     // let x = Serializable(5)
    //     // let json = JsonConvert.SerializeObject(x, JsonSerializerSettings(TypeNameHandling=TypeNameHandling.All))
    //     let json = """{"$type":"Fable.Tests.TypeTests+Serializable","PublicValue":1}"""
    //     #if FABLE_COMPILER
    //     let x2 = Fable.Core.JsInterop.ofJson<Serializable> json
    //     string x2 |> equal "Public: 1 - Private: 0 - Deserialized: true"
    //     let x2 = Fable.Core.JsInterop.ofJsonAsType json typedefof<Serializable>
    //     string x2 |> equal "Public: 1 - Private: 0 - Deserialized: true"
    //     let x2 = Fable.Core.JsInterop.ofJsonWithTypeInfo<Serializable> json
    //     #else
    //     let x2 = Newtonsoft.Json.JsonConvert.DeserializeObject<Serializable> json
    //     #endif
    //     string x2 |> equal "Public: 1 - Private: 0 - Deserialized: true"

    testCase "Secondary constructors work" <| fun () ->
        let s1 = SecondaryCons(3)
        let s2 = SecondaryCons()
        equal 3 s1.Value
        equal 5 s2.Value

    testCase "Multiple constructors work" <| fun () ->
        let m1 = MultipleCons()
        let m2 = MultipleCons(5)
        let m3 = MultipleCons(7,7)
        equal 5 m1.Value
        equal 9 m2.Value
        equal 14 m3.Value

    testCase "Abstract methods with default work" <| fun () -> // See #505
        let x = ConcreteClass()
        x.MethodWithDefault() |> equal "Hello "
        x.MustImplement() |> equal "World!!"
        x.CallMethodWithDefault() |> equal "Hello World!!"
        let x = ConcreteClass2()
        x.CallMethodWithDefault() |> equal "Hi World!!"

    testCase "Interface setters don't conflict" <| fun () -> // See #505
        let x = XISomeInterface () :> ISomeInterface
        x.Sender |> equal 0
        x.Sender <- 5
        x.Sender |> equal 5

    testCase "A type can overload an interface method" <| fun () ->
        let foo = FooImplementor()
        foo.Foo() |> equal "foo"
        (foo :> IFoo).Foo() |> equal "foobar"
        mangleFoo foo |> equal "foobar"

    testCase "A child can be casted to parent's interface" <| fun () ->
        let foo = FooImplementorChild()
        foo.Foo() |> equal "foofoofoo"
        (foo :> IFoo).Foo() |> equal "foofoofoobar"
        mangleFoo foo |> equal "foofoofoobar"

    testCase "A type can overload an interface getter" <| fun () ->
        let foo = FooImplementor()
        foo.Bar |> equal "he"
        (foo :> IFoo).Bar |> equal "heho"

    testCase "A type can overload an interface setter" <| fun () ->
        let foo = FooImplementor()
        foo.MySetter <- 7
        foo.MySetter |> equal 9
        (foo :> IFoo).MySetter <- 7
        (foo :> IFoo).MySetter |> equal 19

    testCase "A type overloading an interface method can be inherited" <| fun () ->
        let foo = ChildFoo() :> AbstractFoo
        foo.Foo() |> equal "BAR"
        (foo :> IFoo).Foo() |> equal "BARFOO"
        mangleFoo foo |> equal "BARFOO"

    testCase "Calling default implementation of base members don't cause infinite recursion" <| fun () -> // See #701
        ExtendedClass().Init() |> equal 7

    testCase "Circular dependencies work" <| fun () -> // See #569
        let location = { name="NY"; employees=[] }
        let alice = { name="Alice"; age=20.0; location=location  }
        location.name |> equal "NY"
        alice.age |> equal 20.

    testCase "Value Types work" <| fun () -> // See #568
        let test = ValueType<_>("foo")
        test.Value |> equal "foo"
        test.foo |> equal "foo"
        let test2 = ValueType2(3, 4)
        test2.Value |> equal 7
        let p = Point2D(2.)
        p.Y |> equal 2.

    testCase "Custom F# exceptions work" <| fun () ->
        try
            MyEx(4,"ERROR") |> raise
        with
        | MyEx(4, msg) as e -> (box e :? Exception, msg + "!!")
        | MyEx(_, msg) as e -> (box e :? Exception, msg + "??")
        | ex -> (false, "unknown")
        |> equal (true, "ERROR!!")

    testCase "Custom exceptions work" <| fun () ->
        try
            MyEx2(5.5) |> raise
        with
        | :? MyEx2 as ex -> (box ex :? Exception, ex.Message, ex.Code)
        | ex -> (false, "unknown", 0.)
        |> equal (true, "Code: 5", 5.5)

    testCase "reraise works" <| fun () ->
        try
            try
                Exception("Will I be reraised?") |> raise
            with _ ->
                try
                    reraise()
                with _ -> reraise()
            "foo"
        with ex -> ex.Message
        |> equal "Will I be reraised?"
  ]