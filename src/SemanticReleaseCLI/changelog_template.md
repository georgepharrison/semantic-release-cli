{% for release in releases %}
# v{{ release.name }}

## {{ release.name }} ({{ release.date }})
{%- for change_type in release.change_types -%}
{%- if change_type.changes_without_scope.size > 0 or change_type.scopes.size > 0 -%}

### {{ change_type.heading }}

{%- for change in change_type.changes_without_scope -%}
- {{ change }}
{%- endfor -%}
{%- for scope in change_type.scopes -%}
- ##### {{ scope.name }}
  {%- for change in scope.changes -%}
  - {{ change }}
  {%- endfor -%}
{%- endfor -%}
{%- endif -%}{%- endfor -%}{%- if releases.size > 1 -%}

---
{%- endif -%}{%- endfor -%}