{% for release in releases %}
# v{{ release.name }}

## {{ release.name }} ({{ release.date }})
{%- if release.features.size > 0 -%}

### Features

{%- for feature in release.features -%}
- {{ feature }}
{%- endfor -%}
{%- endif -%}{%- if release.bug_fixes.size > 0 -%}

### Bug Fixes

{%- for bug_fix in release.bug_fixes -%}
- {{ bug_fix }}
{%- endfor -%}
{%- endif -%}{%- if release.chores.size > 0 -%}

### Chores

{%- for chore in release.chores -%}
- {{ chore }}
{%- endfor -%}
{%- endif -%}{%- if releases.size > 1 -%}

---
{%- endif -%}{%- endfor -%}