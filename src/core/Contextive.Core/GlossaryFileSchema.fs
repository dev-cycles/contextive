module Contextive.Core.GlossaryFileSchema

let schema =
    """{
  "$schema": "http://json-schema.org/draft-07/schema",
  "$id": "https://contextive.tech/",
  "title": "Contextive Glossary Schema",
  "description": "Schema for defining a ubiquitous language glossary.",
  "type": "object",
  "properties": {
    "contexts": {
      "description": "A list of contexts.",
      "type": "array",
      "items": {
        "title": "Context",
        "description": "A context in the ubiquitous language, consisting of a list of terms.",
        "type": "object",
        "properties": {
          "name": {
            "title": "Name",
            "description": "The name of the context.",
            "type": "string",
            "examples": [
              "Cargo"
            ]
          },
          "domainVisionStatement": {
            "title": "Domain Vision Statement",
            "description": "A statement that describes the purpose of the context.",
            "type": "string",
            "examples": [
              "To manage the routing of cargo through transportation legs"
            ]
          },
          "paths": {
            "title": "Paths",
            "description": "A list of paths that the context is involved in.",
            "type": "array",
            "items": {
              "title": "Path",
              "description": "A path that the context is involved in.",
              "type": "string",
              "examples": [
                "CargoDemo",
                "'**' # default, all paths"
              ]
            },
            "examples": [
              [
                "CargoDemo"
              ]
            ],
            "minItems": 1,
            "uniqueItems": true
          },
          "terms": {
            "title": "Terms",
            "description": "A list of terms in the context.",
            "type": "array",
            "items": {
              "title": "Term",
              "description": "A term in the context.",
              "type": "object",
              "properties": {
                "name": {
                  "title": "Name",
                  "description": "The name of the term.",
                  "type": "string",
                  "examples": [
                    "Cargo"
                  ]
                },
                "definition": {
                  "title": "Definition",
                  "description": "A definition of the term.",
                  "type": "string",
                  "examples": [
                    "A unit of transportation that needs moving and delivery to its delivery location."
                  ]
                },
                "examples": {
                  "title": "Examples",
                  "description": "A list of example sentences using the term.",
                  "type": "array",
                  "items": {
                    "title": "Example sentence",
                    "description": "An example sentence using the term.",
                    "type": "string",
                    "examples": [
                      "Multiple Customers are involved with a Cargo, each playing a different role.",
                      "The Cargo delivery goal is specified."
                    ]
                  },
                  "examples": [
                    [
                      "Multiple Customers are involved with a Cargo, each playing a different role.",
                      "The Cargo delivery goal is specified."
                    ]
                  ],
                  "minItems": 1,
                  "uniqueItems": true
                },
                "aliases": {
                  "title": "Aliases",
                  "description": "A list of aliases for the term.",
                  "type": "array",
                  "items": {
                    "title": "Alias",
                    "description": "An alias for the term.",
                    "type": "string",
                    "examples": [
                      "unit"
                    ]
                  },
                  "examples": [
                    [
                      "unit"
                    ]
                  ],
                  "minItems": 1,
                  "uniqueItems": true
                }
              },
              "required": ["name"]
            },
            "examples": [
              {
                "name": "Cargo",
                "definition": "A unit of transportation that needs moving and delivery to its delivery location.",
                "examples": [
                  "Multiple Customers are involved with a Cargo, each playing a different role.",
                  "The Cargo delivery goal is specified."
                ],
                "aliases": [
                  "unit"
                ]
              }
            ],
            "minItems": 1,
            "uniqueItems": true
          }
        },
        "required": ["name", "terms"]
      },
      "minItems": 1,
      "uniqueItems": true,
      "examples": [
        {
          "name": "Cargo",
          "domainVisionStatement": "To manage the routing of cargo through transportation legs",
          "paths": [
            "CargoDemo"
          ],
          "terms": [
            {
              "name": "Cargo",
              "definition": "A unit of transportation that needs moving and delivery to its delivery location.",
              "examples": [
                "Multiple Customers are involved with a Cargo, each playing a different role.",
                "The Cargo delivery goal is specified."
              ],
              "aliases": [
                "unit"
              ]
            }
          ]
        }
      ]
    }
  },
  "required": ["contexts"]
}
"""
