module Seq

let (|Empty|_|) a = if Seq.isEmpty a then Some() else None
