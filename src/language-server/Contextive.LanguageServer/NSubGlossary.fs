module Contextive.LanguageServer.NSubGlossary

type Messages = Create of string

type T = MailboxProcessor<Messages>
