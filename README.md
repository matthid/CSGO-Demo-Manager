# CSGO Demo Manager

This template can be used to generate a full-stack web application using the [SAFE Stack](https://safe-stack.github.io/). It was created using the dotnet [SAFE Template](https://safe-stack.github.io/docs/template-overview/). If you want to learn more about the template why not start with the [quick start](https://safe-stack.github.io/docs/quickstart/) guide?

## Install pre-requisites

You'll need to install the following pre-requisites in order to build CSGO Demo Manager

* The [.NET Core SDK](https://www.microsoft.com/net/download)
* [FAKE 5](https://fake.build/) installed as a [global tool](https://fake.build/fake-gettingstarted.html#Install-FAKE)
* The [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager (you an also use `npm` but the usage of `yarn` is encouraged).
* [Node LTS](https://nodejs.org/en/download/) installed for the front end components.
* If you're running on OSX or Linux, you'll also need to install [Mono](https://www.mono-project.com/docs/getting-started/install/).

## Work with the application

To concurrently run the server and the client components in watch mode use the following command:

```bash
fake build -t Run
```


## Components Documentation

You will find more documentation about the used components at the following places:

* [Giraffe](https://github.com/giraffe-fsharp/Giraffe/blob/master/DOCUMENTATION.md) the backend server technology based on ASP.Net.Core
* [Fable](https://fable.io/docs/) the F# to JS transpiler (frontend)
* [Elmish](https://elmish.github.io/elmish/) the general architecture
* [Fulma](https://fulma.github.io/Fulma/) the bulma based css framework
* [react](https://reactjs.org/) the general view framework
* [react-table](https://www.npmjs.com/package/react-table) to support in tabular views
* [Electron](https://electronjs.org/) to bundle everything as pp

## Troubleshooting

* **fake not found** - If you fail to execute `fake` from command line after installing it as a global tool, you might need to add it to your `PATH` manually: (e.g. `export PATH="$HOME/.dotnet/tools:$PATH"` on unix) - [related GitHub issue](https://github.com/dotnet/cli/issues/9321)

## Why rewrite the frontend?

Original Demo Manager

- is slow for lots of demos
- makes it hard to contribute due to paid telerik components
- doesn't have the analytics I'd like to have
- ... Further ideas

## TODOs

- Make Download MM button work (with old boiler.exe implementation)
- Analyse demos automatically
- Download MM demos after CSGO is closed
- "properly" Support multiple accounts 
- Mark Win/Loss on Demos
- Implement DialogService for notifications
- Details page for demos
- Watch details page/popup -> Launch CSGO
- Heatmap
- Make columns customizable
