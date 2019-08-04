module rec ReactTable

// ts2fable 0.6.2
open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.React

// README:
// 'D is most of the time the type of a specific row (ie the input item)
// 't / 'ColVal is most of the time the type of a specific column (ie the type of a field of 'D)
type ReactNode = ReactElement
type ReactType = ReactElement

type ComponentType<'Props> =
    U2<FunctionComponent<'Props>, Component<'Props, obj>>

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ComponentType =
    let ofFunc f: ComponentType<'Props> = f |> U2.Case1
    let ofComp (c:Component<'Props, 'state>) : ComponentType<'Props> = unbox c |> U2.Case2

type ComponentType = ComponentType<obj>

let _css : obj = importAll "react-table/react-table.css"

let [<Import("ReactTableDefaults","react-table")>] ReactTableDefaults: TableProps = jsNative

type [<AllowNullLiteral>] IExports =
    abstract ReactTable: ReactTableStatic

type [<AllowNullLiteral>] ReactTableFunction =
    [<Emit "$0($1...)">] abstract Invoke: ?value: obj -> unit

type AccessorFunction =
    AccessorFunction<obj, obj>
    
type AccessorFunction<'D> =
    AccessorFunction<'D, obj>

type AccessorFunction<'D, 'ColVal> = delegate of row:'D -> 'ColVal option
    
type Accessor =
    Accessor<obj, obj>

type Accessor<'D> =
    Accessor<'D, obj>

type Accessor<'D, 'ColVal> =
    U3<string, ResizeArray<string>, AccessorFunction<'D, 'ColVal>>

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Accessor =
    let ofString v: Accessor<'D, 'ColVal> = v |> U3.Case1
    let isString (v: Accessor<'D, 'ColVal>) = match v with U3.Case1 _ -> true | _ -> false
    let asString (v: Accessor<'D, 'ColVal>) = match v with U3.Case1 o -> Some o | _ -> None
    let ofStringArray v: Accessor<'D, 'ColVal> = v |> U3.Case2
    let isStringArray (v: Accessor<'D, 'ColVal>) = match v with U3.Case2 _ -> true | _ -> false
    let asStringArray (v: Accessor<'D, 'ColVal>) = match v with U3.Case2 o -> Some o | _ -> None
    let ofAccessorFunction v: Accessor<'D, 'ColVal> = v |> U3.Case3
    let ofFunc v: Accessor<'D, 'ColVal> = AccessorFunction<'D, 'ColVal> v |> U3.Case3
    let isAccessorFunction (v: Accessor<'D, 'ColVal>) = match v with U3.Case3 _ -> true | _ -> false
    let asAccessorFunction (v: Accessor<'D, 'ColVal>) = match v with U3.Case3 o -> Some o | _ -> None

type Aggregator = delegate of values:obj option * rows:obj option -> obj option

type TableCellRenderer<'D, 't> =
    U2<(CellInfo<'D, 't> -> ReactElement), ReactElement>

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module TableCellRenderer =
    let ofCase1 v: TableCellRenderer<'D,'t> = v |> U2.Case1
    let isCase1 (v: TableCellRenderer<'D,'t>) = match v with U2.Case1 _ -> true | _ -> false
    let asCase1 (v: TableCellRenderer<'D,'t>) = match v with U2.Case1 o -> Some o | _ -> None
    let ofReactElement v: TableCellRenderer<'D,'t> = v |> U2.Case2

type [<AllowNullLiteral>] FilterRender =
    [<Emit "$0($1...)">] abstract Invoke: ``params``: FilterRenderInvokeParams -> ReactElement

type [<AllowNullLiteral>] FilterRenderInvokeParams =
    abstract column: Column with get, set
    abstract filter: obj option with get, set
    abstract onChange: ReactTableFunction with get, set
    abstract key: string option with get, set

type PivotRenderer<'D, 't> =
    U4<(CellInfo<'D, 't> -> ReactElement), (unit -> obj option), string, ReactElement>

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PivotRenderer =
    let ofCase1 v: PivotRenderer<'D,'t> = v |> U4.Case1
    let isCase1 (v: PivotRenderer<'D,'t>) = match v with U4.Case1 _ -> true | _ -> false
    let asCase1 (v: PivotRenderer<'D,'t>) = match v with U4.Case1 o -> Some o | _ -> None
    let ofCase2 v: PivotRenderer<'D,'t> = v |> U4.Case2
    let isCase2 (v: PivotRenderer<'D,'t>) = match v with U4.Case2 _ -> true | _ -> false
    let asCase2 (v: PivotRenderer<'D,'t>) = match v with U4.Case2 o -> Some o | _ -> None
    let ofString v: PivotRenderer<'D,'t> = v |> U4.Case3
    let isString (v: PivotRenderer<'D,'t>) = match v with U4.Case3 _ -> true | _ -> false
    let asString (v: PivotRenderer<'D,'t>) = match v with U4.Case3 o -> Some o | _ -> None
    let ofReactElement v: PivotRenderer<'D,'t> = v |> U4.Case4

type ComponentPropsGetter0 = delegate of finalState: obj option * rowInfo: obj * column: obj * ?instance: obj -> obj option
type ComponentPropsGetterR<'D> = delegate of finalState: obj option * ?rowInfo: RowInfo<'D> * ?column: obj * ?instance: obj -> obj option
type ComponentPropsGetterC = delegate of finalState: obj option * ?rowInfo: obj * ?column: Column * ?instance: obj -> obj option
type ComponentPropsGetterRC<'D> = delegate of finalState: obj option * ?rowInfo: RowInfo<'D> * ?column: Column * ?instance: obj -> obj option
type DefaultFilterFunction = delegate of filter: Filter * row: obj option * column: obj option -> bool
type FilterFunction = delegate of filter: Filter * rows: ResizeArray<obj option> * column: obj option -> ResizeArray<obj option>
type SubComponentFunction<'D> = delegate of rowInfo: RowInfo<'D> -> ReactElement
type PageChangeFunction = delegate of page: float -> unit
type PageSizeChangeFunction = delegate of newPageSize: float * newPage: float -> unit
type SortedChangeFunction = delegate of newSorted: ResizeArray<SortingRule> * column: obj option * additive: bool -> unit
type FilteredChangeFunction = delegate of newFiltering: ResizeArray<Filter> * column: obj option * value: obj option -> unit
type ExpandedChangeFunction = delegate of column: obj option * ``event``: obj option * isTouch: bool -> unit
type ResizedChangeFunction = delegate of newResized: ResizeArray<Resize> * ``event``: obj option -> unit
type SortFunction = delegate of a: obj option * b: obj option * desc: obj option -> float

type [<AllowNullLiteral>] Resize =
    abstract id: string with get, set
    abstract value: obj option with get, set

type [<AllowNullLiteral>] Filter =
    abstract id: string with get, set
    abstract value: obj option with get, set
    abstract pivotId: string option with get, set

type [<AllowNullLiteral>] SortingRule =
    abstract id: string with get, set
    abstract desc: bool with get, set

type TableProps<'ResolvedData> =
    TableProps<obj, 'ResolvedData>

type TableProps =
    TableProps<obj, obj>

type [<AllowNullLiteral>] TableProps<'D, 'ResolvedData> =
    inherit TextProps
    inherit ComponentDecoratorProps<'D>
    inherit ControlledStateCallbackProps
    inherit PivotingProps
    inherit ControlledStateOverrideProps<'D>
    inherit ComponentProps
    /// Default: [] 
    abstract data: ResizeArray<'D> with get, set
    abstract resolveData: (ResizeArray<'D> -> ResizeArray<'ResolvedData>) option with get, set
    /// Default: false 
    abstract loading: bool with get, set
    /// Default: true 
    abstract showPagination: bool with get, set
    /// Default: false 
    abstract showPaginationTop: bool with get, set
    /// Default: true  
    abstract showPaginationBottom: bool with get, set
    /// Default: false 
    abstract manual: bool with get, set
    /// Default: true 
    abstract multiSort: bool with get, set
    /// Default: true 
    abstract showPageSizeOptions: bool with get, set
    /// Default: [5, 10, 20, 25, 50, 100] 
    abstract pageSizeOptions: ResizeArray<float> with get, set
    /// Default: 20 
    abstract defaultPageSize: float with get, set
    /// Default: undefined
    /// Otherwise take value from 'pageSize' if defined
    abstract minRows: float option with get, set
    /// Default: true 
    abstract showPageJump: bool with get, set
    /// Default: true 
    abstract sortable: bool with get, set
    /// Default: true 
    abstract collapseOnSortingChange: bool with get, set
    /// Default: true 
    abstract collapseOnPageChange: bool with get, set
    /// Default: true 
    abstract collapseOnDataChange: bool with get, set
    /// Default: false 
    abstract freezeWhenExpanded: bool with get, set
    /// Default: [] 
    abstract defaultSorting: ResizeArray<SortingRule> with get, set
    /// Default: false 
    abstract showFilters: bool with get, set
    /// Default: [] 
    abstract defaultFiltering: ResizeArray<Filter> with get, set
    /// Default: ... 
    abstract defaultFilterMethod: DefaultFilterFunction with get, set
    /// Default: ... 
    abstract defaultSortMethod: SortFunction with get, set
    /// Default: true 
    abstract resizable: bool with get, set
    /// Default: false 
    abstract filterable: bool with get, set
    /// Default: [] 
    abstract defaultResizing: ResizeArray<Resize> with get, set
    /// Default: false 
    abstract defaultSortDesc: bool with get, set
    /// Default: [] 
    abstract defaultSorted: ResizeArray<SortingRule> with get, set
    /// Default: [] 
    abstract defaultFiltered: ResizeArray<Filter> with get, set
    /// Default: [] 
    abstract defaultResized: ResizeArray<Resize> with get, set
    /// Default: {} 
    abstract defaultExpanded: TypeLiteral_01 with get, set
    /// On change. 
    abstract onChange: ReactTableFunction with get, set
    /// Default: string
    /// Adding a -striped className to ReactTable will slightly color odd numbered rows for legibility
    /// Adding a -highlight className to ReactTable will highlight any row as you hover over it
    abstract className: string with get, set
    /// Default: {} 
    abstract style: obj with get, set
    /// Global Column Defaults 
    abstract column: obj with get, set
    /// Array of all Available Columns 
    abstract columns: Column<'ResolvedData, obj>[] option with get, set
    /// Expander defaults. 
    abstract expanderDefaults: obj with get, set
    /// Privot defaults. 
    abstract pivotDefaults: obj with get, set
    /// The content rendered inside of a padding row 
    abstract PadRowComponent: (unit -> ReactElement) with get, set
    /// Server-side callbacks 
    abstract onFetchData: (FetchDataState -> obj option -> unit) with get, set
    /// Control callback for functional rendering 
    abstract children: (FinalState<'ResolvedData> -> (unit -> ReactElement) -> Instance<'ResolvedData> -> ReactElement) with get, set

type [<AllowNullLiteral>] ControlledStateOverrideProps<'D> =
    /// Default: undefined 
    abstract page: float option with get, set
    /// Default: undefined 
    abstract pageSize: float option with get, set
    /// Default: undefined 
    abstract pages: float option with get, set
    /// Default: undefined 
    abstract sorting: float with get, set
    /// Default: [] 
    abstract sorted: ResizeArray<SortingRule> with get, set
    /// Default: [] 
    abstract filtered: ResizeArray<Filter> with get, set
    /// Default: [] 
    abstract resized: ResizeArray<Resize> with get, set
    /// Default: {} 
    abstract expanded: TypeLiteral_01 with get, set
    /// Sub component 
    abstract SubComponent: SubComponentFunction<'D> with get, set
    
type [<AllowNullLiteral>] FetchDataState =
    /// Default: undefined 
    abstract page: float with get, set
    /// Default: undefined 
    abstract pageSize: float with get, set
    /// Default: [] 
    abstract sorted: ResizeArray<SortingRule> with get, set
    /// Default: [] 
    abstract filtered: ResizeArray<Filter> with get, set
    
type [<AllowNullLiteral>] PivotingProps =
    /// Default: undefined 
    abstract pivotBy: ResizeArray<string> with get, set
    /// Default: 200 
    abstract pivotColumnWidth: float with get, set
    /// Default: _pivotVal 
    abstract pivotValKey: string with get, set
    /// Default: _pivotID 
    abstract pivotIDKey: string with get, set
    /// Default: _subRows 
    abstract subRowsKey: string with get, set
    /// Default: _aggregated 
    abstract aggregatedKey: string with get, set
    /// Default: _nestingLevel 
    abstract nestingLevelKey: string with get, set
    /// Default: _original 
    abstract originalKey: string with get, set
    /// Default: _index 
    abstract indexKey: string with get, set
    /// Default: _groupedByPivot 
    abstract groupedByPivotKey: string with get, set
    /// Default: {} - Pivoting State Overrides (see Fully Controlled Component section)
    abstract expandedRows: ExpandedRows with get, set
    /// Default: ??? - Pivoting State Callbacks 
    abstract onExpandRow: ReactTableFunction with get, set

type [<AllowNullLiteral>] ExpandedRows =
    [<Emit "$0[$1]{{=$2}}">] abstract Item: idx: float -> U2<bool, ExpandedRows> with get, set

type [<AllowNullLiteral>] DerivedDataObject =
    abstract _index: float with get, set
    abstract _nestingLevel: float with get, set
    abstract _subRows: obj option with get, set
    abstract _original: obj option with get, set
    [<Emit "$0[$1]{{=$2}}">] abstract Item: p: string -> obj option with get, set

type [<AllowNullLiteral>] ControlledStateCallbackProps =
    /// Called when the page index is changed by the user 
    abstract onPageChange: PageChangeFunction with get, set
    /// Called when the pageSize is changed by the user. The resolve page is also sent
    ///   to maintain approximate position in the data
    abstract onPageSizeChange: PageSizeChangeFunction with get, set
    /// Called when a sortable column header is clicked with the column itself and if
    /// the shiftkey was held. If the column is a pivoted column, `column` will be an array of columns
    abstract onSortedChange: SortedChangeFunction with get, set
    /// Called when a user enters a value into a filter input field or the value passed
    /// to the onFiltersChange handler by the Filter option.
    abstract onFilteredChange: FilteredChangeFunction with get, set
    /// Called when an expander is clicked. Use this to manage `expanded` 
    abstract onExpandedChange: ExpandedChangeFunction with get, set
    /// Called when a user clicks on a resizing component (the right edge of a column header) 
    abstract onResizedChange: ResizedChangeFunction with get, set

type [<AllowNullLiteral>] ComponentDecoratorProps<'D> =
    abstract getProps: U3<ComponentPropsGetterRC<'D>, ComponentPropsGetterC, ComponentPropsGetter0> with get, set
    abstract getTableProps: ComponentPropsGetter0 with get, set
    abstract getTheadGroupProps: ComponentPropsGetter0 with get, set
    abstract getTheadGroupTrProps: ComponentPropsGetter0 with get, set
    abstract getTheadGroupThProps: ComponentPropsGetterC with get, set
    abstract getTheadProps: ComponentPropsGetter0 with get, set
    abstract getTheadTrProps: ComponentPropsGetter0 with get, set
    abstract getTheadThProps: ComponentPropsGetterC with get, set
    abstract getTheadFilterProps: ComponentPropsGetter0 with get, set
    abstract getTheadFilterTrProps: ComponentPropsGetter0 with get, set
    abstract getTheadFilterThProps: ComponentPropsGetterC with get, set
    abstract getTbodyProps: ComponentPropsGetter0 with get, set
    abstract getTrGroupProps: U2<ComponentPropsGetterR<'D>, ComponentPropsGetter0> with get, set
    abstract getTrProps: U2<ComponentPropsGetterR<'D>, ComponentPropsGetter0> with get, set
    abstract getTdProps: U2<ComponentPropsGetterRC<'D>, ComponentPropsGetterR<'D>> with get, set
    abstract getTfootProps: ComponentPropsGetter0 with get, set
    abstract getTfootTrProps: ComponentPropsGetter0 with get, set
    abstract getTfootTdProps: ComponentPropsGetterC with get, set
    abstract getPaginationProps: ComponentPropsGetter0 with get, set
    abstract getLoadingProps: ComponentPropsGetter0 with get, set
    abstract getNoDataProps: ComponentPropsGetter0 with get, set
    abstract getResizerProps: ComponentPropsGetter0 with get, set

type [<AllowNullLiteral>] ComponentProps =
    abstract TableComponent: ReactType with get, set
    abstract TheadComponent: ReactType with get, set
    abstract TbodyComponent: ReactType with get, set
    abstract TrGroupComponent: ReactType with get, set
    abstract TrComponent: ReactType with get, set
    abstract ThComponent: ReactType with get, set
    abstract TdComponent: ReactType with get, set
    abstract TfootComponent: ReactType with get, set
    abstract ExpanderComponent: ReactType with get, set
    abstract AggregatedComponent: ReactType with get, set
    abstract PivotValueComponent: ReactType with get, set
    abstract PivotComponent: ReactType with get, set
    abstract FilterComponent: ReactType with get, set
    abstract PaginationComponent: ReactType with get, set
    abstract PreviousComponent: ReactType with get, set
    abstract NextComponent: ReactType with get, set
    abstract LoadingComponent: ReactType with get, set
    abstract NoDataComponent: ReactType with get, set
    abstract ResizerComponent: ReactType with get, set

type [<AllowNullLiteral>] TextProps =
    /// Default: 'Previous' 
    abstract previousText: ReactElement with get, set
    /// Default: 'Next' 
    abstract nextText: ReactElement with get, set
    /// Default: 'Loading...' 
    abstract loadingText: ReactElement with get, set
    /// Default: 'No rows found' 
    abstract noDataText: U2<ReactElement, ComponentType> with get, set
    /// Default: 'Page' 
    abstract pageText: ReactElement with get, set
    /// Default: 'of' 
    abstract ofText: ReactElement with get, set
    /// Default: 'rows' 
    abstract rowsText: string with get, set

type [<AllowNullLiteral>] GlobalColumn<'D, 't> =
    inherit Column.Basics<'D, 't>
    inherit Column.CellProps<'D, 't>
    inherit Column.FilterProps
    inherit Column.FooterProps<'D, 't>
    inherit Column.HeaderProps<'D, 't>

type [<AllowNullLiteral>] ExpanderDefaults =
    /// Default: false 
    abstract sortable: bool with get, set
    /// Default: false 
    abstract resizable: bool with get, set
    /// Default: false 
    abstract filterable: bool with get, set
    /// Default: 35 
    abstract width: float with get, set

type [<AllowNullLiteral>] PivotDefaults<'D> =
    /// Will be overriden in methods.js to display ExpanderComponent 
    abstract render: TableCellRenderer<'D, obj> with get, set

type Column =
    Column<obj>

type Column<'D> =
    Column<'D, obj>

module Column =
    /// Basic column props 
    type [<AllowNullLiteral>] Basics<'D, 't> =
        /// Default: undefined, use table default 
        abstract sortable: bool option with get, set
        /// Default: true 
        abstract show: bool with get, set
        /// Default: 100 
        abstract minWidth: float with get, set
        /// Default: undefined, use table default 
        abstract resizable: bool option with get, set
        /// Default: undefined, use table default 
        abstract filterable: bool option with get, set
        /// Default: ... 
        abstract sortMethod: SortFunction option with get, set
        /// Default: false 
        abstract defaultSortDesc: bool option with get, set
        /// Used to render aggregated cells. Defaults to a comma separated list of values. 
        abstract Aggregated: TableCellRenderer<'D, 't> with get, set
        /// Used to render a pivoted cell  
        abstract Pivot: PivotRenderer<'D, 't> with get, set
        /// Used to render the value inside of a Pivot cell 
        abstract PivotValue: TableCellRenderer<'D, 't> with get, set
        /// Used to render the expander in both Pivot and Expander cells 
        abstract Expander: TableCellRenderer<'D, 't> with get, set
    
    /// Basic column props 
    type [<AllowNullLiteral>] PartialBasics<'D, 't> =
        /// Default: undefined, use table default 
        abstract sortable: bool option with get, set
        /// Default: true 
        abstract show: bool option with get, set
        /// Default: 100 
        abstract minWidth: float option with get, set
        /// Default: undefined, use table default 
        abstract resizable: bool option with get, set
        /// Default: undefined, use table default 
        abstract filterable: bool option with get, set
        /// Default: ... 
        abstract sortMethod: SortFunction option with get, set
        /// Default: false 
        abstract defaultSortDesc: bool option with get, set
        /// Used to render aggregated cells. Defaults to a comma separated list of values. 
        abstract Aggregated: TableCellRenderer<'D, 't> option with get, set
        /// Used to render a pivoted cell  
        abstract Pivot: PivotRenderer<'D, 't> option with get, set
        /// Used to render the value inside of a Pivot cell 
        abstract PivotValue: TableCellRenderer<'D, 't> option with get, set
        /// Used to render the expander in both Pivot and Expander cells 
        abstract Expander: TableCellRenderer<'D, 't> option with get, set

    /// Configuration of a columns cell section 
    type [<AllowNullLiteral>] CellProps<'D, 't> =
        /// Default: undefined
        /// A function that returns a primitive, or JSX / React Component
        abstract Cell: TableCellRenderer<'D, 't> with get, set
        /// Set the classname of the `td` element of the column
        abstract className: string with get, set
        /// Set the style of the `td` element of the column
        abstract style: obj with get, set
        abstract getProps: ReactTableFunction with get, set
    
    /// Configuration of a columns cell section 
    type [<AllowNullLiteral>] PartialCellProps<'D, 't> =
        /// Default: undefined
        /// A function that returns a primitive, or JSX / React Component
        abstract Cell: TableCellRenderer<'D, 't> option with get, set
        /// Set the classname of the `td` element of the column
        abstract className: string option with get, set
        /// Set the style of the `td` element of the column
        abstract style: obj option with get, set
        abstract getProps: ReactTableFunction option with get, set

    /// Configuration of a columns header section 
    type [<AllowNullLiteral>] HeaderProps<'D, 't> =
        /// Default: undefined
        /// A function that returns a primitive, or JSX / React Component
        abstract Header: TableCellRenderer<'D, 't> with get, set
        /// Set the classname of the `th` element of the column
        abstract headerClassName: string with get, set
        /// Default: {}
        /// Set the style of the `th` element of the column
        abstract headerStyle: obj with get, set
        /// Default: (state, rowInfo, column, instance) => ({})
        /// A function that returns props to decorate the `th` element of the column
        abstract getHeaderProps: ReactTableFunction with get, set
    
    /// Configuration of a columns header section 
    type [<AllowNullLiteral>] PartialHeaderProps<'D, 't> =
        /// Default: undefined
        /// A function that returns a primitive, or JSX / React Component
        abstract Header: TableCellRenderer<'D, 't> option with get, set
        /// Set the classname of the `th` element of the column
        abstract headerClassName: string option with get, set
        /// Default: {}
        /// Set the style of the `th` element of the column
        abstract headerStyle: obj option with get, set
        /// Default: (state, rowInfo, column, instance) => ({})
        /// A function that returns props to decorate the `th` element of the column
        abstract getHeaderProps: ReactTableFunction option with get, set

    /// Configuration of a columns footer section 
    type [<AllowNullLiteral>] FooterProps<'D, 't> =
        /// Default: undefined
        /// A function that returns a primitive, or JSX / React Component
        abstract Footer: TableCellRenderer<'D, 't> with get, set
        /// Default: string
        /// Set the classname of the `td` element of the column's footer
        abstract footerClassName: string with get, set
        /// Default: {}
        /// Set the style of the `td` element of the column's footer
        abstract footerStyle: obj with get, set
        /// Default: (state, rowInfo, column, instance) => ({})
        /// A function that returns props to decorate the `th` element of the column
        abstract getFooterProps: ReactTableFunction with get, set
    
    /// Configuration of a columns footer section 
    type [<AllowNullLiteral>] PartialFooterProps<'D, 't> =
        /// Default: undefined
        /// A function that returns a primitive, or JSX / React Component
        abstract Footer: TableCellRenderer<'D, 't> option with get, set
        /// Default: string
        /// Set the classname of the `td` element of the column's footer
        abstract footerClassName: string option with get, set
        /// Default: {}
        /// Set the style of the `td` element of the column's footer
        abstract footerStyle: obj option with get, set
        /// Default: (state, rowInfo, column, instance) => ({})
        /// A function that returns props to decorate the `th` element of the column
        abstract getFooterProps: ReactTableFunction option with get, set

    /// Filtering related column props 
    type [<AllowNullLiteral>] FilterProps =
        /// Default: false 
        abstract filterAll: bool with get, set
        /// A function returning a boolean that specifies the filtering logic for the column
        /// 'filter' == an object specifying which filter is being applied. Format: {id: [the filter column's id], value: [the value the user typed in the filter field],
        /// pivotId: [if filtering on a pivot column, the pivotId will be set to the pivot column's id and the `id` field will be set to the top level pivoting column]}
        /// 'row' || 'rows' == the row (or rows, if filterAll is set to true) of data supplied to the table
        /// 'column' == the column that the filter is on
        abstract filterMethod: U2<FilterFunction, DefaultFilterFunction> with get, set
        /// Default: false 
        abstract hideFilter: bool with get, set
        /// Default: ... 
        abstract Filter: FilterRender with get, set

    /// Filtering related column props 
    type [<AllowNullLiteral>] PartialFilterProps =
        /// Default: false 
        abstract filterAll: bool option with get, set
        /// A function returning a boolean that specifies the filtering logic for the column
        /// 'filter' == an object specifying which filter is being applied. Format: {id: [the filter column's id], value: [the value the user typed in the filter field],
        /// pivotId: [if filtering on a pivot column, the pivotId will be set to the pivot column's id and the `id` field will be set to the top level pivoting column]}
        /// 'row' || 'rows' == the row (or rows, if filterAll is set to true) of data supplied to the table
        /// 'column' == the column that the filter is on
        abstract filterMethod: U2<FilterFunction, DefaultFilterFunction> option with get, set
        /// Default: false 
        abstract hideFilter: bool option with get, set
        /// Default: ... 
        abstract Filter: FilterRender option with get, set

type [<AllowNullLiteral>] Column<'D, 'ColVal> =
    inherit Column.PartialBasics<'D, 'ColVal>
    inherit Column.PartialCellProps<'D, 'ColVal>
    inherit Column.PartialFilterProps
    inherit Column.PartialFooterProps<'D, 'ColVal>
    inherit Column.PartialHeaderProps<'D, 'ColVal>
    /// Property name as string or Accessor
    abstract accessor: Accessor<'D, 'ColVal> option with get, set
    /// Conditional - A unique ID is required if the accessor is not a string or if you would like to override the column name used in server-side calls
    abstract id: string option with get, set
    /// No description
    abstract aggregate: Aggregator option with get, set
    /// Default: undefined - A hardcoded width for the column. This overrides both min and max width options
    abstract width: float option with get, set
    /// Default: undefined - A maximum width for this column.
    abstract maxWidth: float option with get, set
    /// Turns this column into a special column for specifying expander and pivot column options.
    /// If this option is true and there is NOT a pivot column, the `expanderDefaults` options will be applied on top of the column options.
    /// If this option is true and there IS a pivot column, the `pivotDefaults` options will be applied on top of the column options.
    /// Adding a column with the `expander` option set will allow you to rearrange expander and pivot column orderings in the table.
    /// It will also let you specify rendering of the header (and header group if this special column is placed in the `columns` option of another column) and the rendering of the expander itself.
    abstract expander: bool option with get, set
    /// Header Groups only 
    abstract columns: Column<'D>[] option with get, set
    /// Turns this column into a special column for specifying pivot position in your column definitions.
    /// The `pivotDefaults` options will be applied on top of this column's options.
    /// It will also let you specify rendering of the header (and header group if this special column is placed in the `columns` option of another column)
    abstract pivot: bool option with get, set

type ColumnRenderProps =
    ColumnRenderProps<obj>

type [<AllowNullLiteral>] ColumnRenderProps<'D> =
    /// Sorted data. 
    abstract data: ResizeArray<'D> with get, set
    /// The column. 
    abstract column: Column<'D> with get, set

type [<AllowNullLiteral>] RowRenderProps =
    //inherit obj
    /// Whenever the current row is expanded 
    abstract isExpanded: bool option with get, set
    /// The current cell value 
    abstract value: obj option with get, set

type [<AllowNullLiteral>] RowInfo<'D> =
    /// Materialized row of data 
    abstract row: obj option with get, set
    /// The post-accessed values from the original row 
    abstract rowValues: obj option with get, set
    /// The index of the row 
    abstract index: float with get, set
    /// The index of the row relative to the current page 
    abstract viewIndex: float with get, set
    /// The size of the page 
    abstract pageSize: float with get, set
    /// The index of page 
    abstract page: float with get, set
    /// The nesting depth (zero-indexed) 
    abstract level: float with get, set
    /// The nesting path of the row 
    abstract nestingPath: ResizeArray<float> with get, set
    /// A boolean stating if the row is an aggregation row 
    abstract aggregated: bool with get, set
    /// A boolean stating if the row is grouped by Pivot 
    abstract groupedByPivot: bool with get, set
    /// An array of any expandable sub-rows contained in this row 
    abstract subRows: ResizeArray<obj option> with get, set
    /// Original object passed to row 
    abstract original: 'D option with get, set

type [<AllowNullLiteral>] CellInfo<'D, 't> =
    inherit RowInfo<'D>
    //inherit obj
    abstract isExpanded: bool with get, set
    abstract column: Column with get, set
    abstract value: 't option with get, set
    abstract pivoted: bool with get, set
    abstract expander: bool with get, set
    abstract show: bool with get, set
    abstract width: float with get, set
    abstract maxWidth: float with get, set
    abstract tdProps: obj option with get, set
    abstract columnProps: obj option with get, set
    abstract classes: ResizeArray<string> with get, set
    abstract styles: obj with get, set

type FinalState =
    FinalState<obj>

type [<AllowNullLiteral>] FinalState<'D> =
    inherit TableProps<'D>
    abstract frozen: bool with get, set
    abstract startRow: float with get, set
    abstract endRow: float with get, set
    abstract padRows: float with get, set
    abstract hasColumnFooter: bool with get, set
    abstract hasHeaderGroups: bool with get, set
    abstract canPrevious: bool with get, set
    abstract canNext: bool with get, set
    abstract rowMinWidth: float with get, set
    abstract allVisibleColumns: Column<'D>[] with get, set
    abstract allDecoratedColumns: Column<'D>[] with get, set
    abstract pageRows: ResizeArray<DerivedDataObject> with get, set
    abstract resolvedData: ResizeArray<DerivedDataObject> with get, set
    abstract sortedData: ResizeArray<DerivedDataObject> with get, set
    abstract headerGroups: ResizeArray<obj option> with get, set

type (* [<AllowNullLiteral>]*) ReactTable(*<'D>*) = ReactElement
    //inherit Component<obj>

type [<AllowNullLiteral>] ReactTableStatic =
    [<Emit "new $0($1...)">] abstract Create: unit -> ReactTable(*<'D>*)

type Instance =
    Instance<obj>

type [<AllowNullLiteral>] Instance<'D> =
    inherit ReactTable(*<'D>*)
    abstract context: obj option with get, set
    abstract props: obj with get, set
    abstract refs: obj option with get, set
    abstract state: FinalState<'D> with get, set
    abstract filterColumn: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract filterData: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract fireFetchData: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract getDataModel: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract getMinRows: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract getPropOrState: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract getResolvedState: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract getSortedData: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract getStateOrProp: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract onPageChange: PageChangeFunction with get, set
    abstract onPageSizeChange: PageSizeChangeFunction with get, set
    abstract resizeColumnEnd: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract resizeColumnMoving: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract resizeColumnStart: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract sortColumn: [<ParamArray>] props: ResizeArray<obj option> -> obj option
    abstract sortData: [<ParamArray>] props: ResizeArray<obj option> -> obj option

type [<AllowNullLiteral>] TypeLiteral_01 =
    interface end

type [<StringEnum>] [<RequireQualifiedAccess>] SelectType =
    | Checkbox
    | Radio

type [<AllowNullLiteral>] SelectInputComponentProps =
    abstract selectType: SelectType with get, set
    /// onClick: (key: string, shiftKeyPressed: boolean, row: any) => any;
    abstract onClick: (string -> bool -> obj option -> unit) with get, set
    abstract ``checked``: bool with get, set
    abstract id: string with get, set
    abstract row: obj option with get, set

type [<AllowNullLiteral>] SelectAllInputComponentProps =
    abstract selectType: SelectType with get, set
    abstract ``checked``: bool with get, set
    abstract onClick: (unit -> obj option) with get, set

type SelectTableProps<'data, 'resolvedData> =
    inherit TableProps<'data, 'resolvedData>
    
    /// Default: _id
    abstract keyField: string option with get, set
    abstract isSelected: (string -> bool) option with get, set
    abstract selectAll: bool option with get, set
    abstract toggleAll: (unit -> unit) option with get, set
    /// onClick: (key: string, shiftKeyPressed: boolean, row: any) => any;
    abstract toggleSelection: (string -> bool -> obj option -> unit) option with get, set
    /// Default: checkbox
    abstract selectType: SelectType option with get, set
    abstract selectWidth: float option with get, set
    abstract SelectInputComponent: ComponentType<SelectInputComponentProps> option with get, set
    abstract SelectAllInputComponent: ComponentType<SelectAllInputComponentProps> option with get, set

type ITableCreator = interface end
    //[<Emit "new $0($1...)">] abstract Create: unit -> ReactTable(*<'D>*)

// https://github.com/fable-compiler/fable-react/issues/61
let RawTable : ITableCreator = import "default" "react-table"
let inline hocTable path : ITableCreator -> ITableCreator = import "default" path
let selectTableFunc table = hocTable "react-table/lib/hoc/selectTable" table

let createTable (tableCreator:ITableCreator) (props:TableProps<'data, 'resolvedData>) = 
    ReactBindings.React.createElement(tableCreator, props, [])
let selectTable (props:SelectTableProps<'data, 'resolvedData>) = createTable (selectTableFunc RawTable) props

let table (props:TableProps<'data, 'resolvedData>) = createTable RawTable props