import js from "@eslint/js";

export default [
    js.configs.recommended,
    {
        files: ["wwwroot/js/**/*.js"],
        languageOptions: {
            ecmaVersion: 2020,
            sourceType: "script",
            globals: {
                // Browser built-ins
                window:               "readonly",
                document:             "readonly",
                navigator:            "readonly",
                localStorage:         "readonly",
                sessionStorage:       "readonly",
                console:              "readonly",
                setTimeout:           "readonly",
                setInterval:          "readonly",
                clearTimeout:         "readonly",
                clearInterval:        "readonly",
                fetch:                "readonly",
                AbortSignal:          "readonly",
                URLSearchParams:      "readonly",
                URL:                  "readonly",
                Blob:                 "readonly",
                Uint8Array:           "readonly",
                Event:                "readonly",
                Notification:         "readonly",
                atob:                 "readonly",
                IntersectionObserver: "readonly",
                MutationObserver:     "readonly",
                // CDN globals
                Chart:    "readonly",
                Sortable: "readonly",
                Blazor:   "readonly",
                // App globals (defined on window in other files)
                chatDial: "readonly",
            }
        },
        rules: {
            // Bug catchers — errors
            "eqeqeq":       ["error", "always"],
            "no-undef":     "error",

            // Style — warnings only (existing code uses var extensively)
            "no-var":        "warn",
            "no-console":    "warn",
            "no-unused-vars": ["warn", { "argsIgnorePattern": "^_", "varsIgnorePattern": "^_" }],
        }
    }
];
