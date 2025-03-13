module Contextive.LanguageServer.Glossary

// starting life as a passthrough wrapper of subglossary
// will become a collection of subglossaries, and will handle identifying the correct subglossary for a given request (hover or completion)

type T = SubGlossary.T

let create = SubGlossary.create

let init = SubGlossary.init

// will trigger an update the default glossary
let onDefaultGlossaryFileLocationChanged = SubGlossary.loader

// will identify which glossary has changed and reload it
let onGlossaryFileChanged = SubGlossary.loader

let find = SubGlossary.find

