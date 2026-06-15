import type { LocationResults } from './HeroSection'
import ResultsSection from './ResultsSection'
import './SearchResults.css'

interface Props {
  results: LocationResults
  onRetry: (location: string) => void
}

export default function SearchResults({ results, onRetry }: Props) {
  if (results.length === 0) return null

  return (
    <div className="search-results">
      {results.map(({ location, state }) => (
        <ResultsSection
          key={location}
          location={location}
          state={state}
          onRetry={onRetry}
        />
      ))}
    </div>
  )
}
