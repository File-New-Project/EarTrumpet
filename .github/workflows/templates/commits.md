{{ range .data.repository.object.history.nodes }}
  {{ $headline := .messageHeadline }}
  {{- range .associatedPullRequests.nodes }}
* {{$headline}}
  {{- else }}
* {{$headline}} ({{ slice .oid 0 7 }})
  {{- end }}
{{- end }}
