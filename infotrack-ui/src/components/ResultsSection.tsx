import type { LocationResult } from './HeroSection'
import SolicitorCard from './SolicitorCard'

interface Props {
  location: string
  state: LocationResult
  onRetry: (location: string) => void
}

export default function ResultsSection({ location, state, onRetry }: Props) {
  return (
    <section className="results-section">
      <h2 className="results-heading">
        <span className="results-location">{location}</span>
        {state.kind === 'loaded' && (
          <span className="results-count">{state.solicitors.length}</span>
        )}
      </h2>
      {renderBody()}
    </section>
  )

  function renderBody() {
    switch (state.kind) {
      case 'loading':
        return <p className="results-note">Searching…</p>
      case 'loaded':
        return state.solicitors.length === 0 ? (
          <p className="results-note">No solicitors found.</p>
        ) : (
          <div className="solicitor-grid">
            {[...state.solicitors]
              .sort((a, b) => a.name.localeCompare(b.name))
              .map((s, i) => (
                <SolicitorCard key={`${s.name}-${i}`} solicitor={s} />
              ))}
          </div>
        )
      case 'not-loaded':
        return (
          <div className="results-note results-note--pending">
            <span>⏳ Not loaded yet.</span>
            <button
              type="button"
              className="results-retry"
              onClick={() => onRetry(location)}
            >
              Retry
            </button>
          </div>
        )
      case 'invalid':
        return <p className="results-note results-note--error">⚠ Not a valid location.</p>
      case 'error':
        return (
          <div className="results-note results-note--error">
            <span>⚠ Couldn’t load.</span>
            <button
              type="button"
              className="results-retry"
              onClick={() => onRetry(location)}
            >
              Retry
            </button>
          </div>
        )
    }
  }
}
