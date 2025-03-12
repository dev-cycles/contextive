module Contextive.LanguageServer.Glossary

// starting life as a passthrough wrapper of subglossary
// will become a collection of subglossaries, and will handle identifying the correct subglossary for a given request (hover or completion)

type T = SubGlossary.T

let create = SubGlossary.create

let init = SubGlossary.init

let reloader = SubGlossary.loader

let find = SubGlossary.find

