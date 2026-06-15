import { useState, useEffect } from 'react'
import LocationTagInput from './LocationTagInput'
import SearchResults from './SearchResults'
import './HeroSection.css'

const API_BASE = import.meta.env.VITE_API_URL ?? ''

export type Solicitor = {
  name: string
  address?: string
  phone?: string
  description?: string
  website?: string
}

export type LocationResult =
  | { kind: 'loading' }
  | { kind: 'loaded'; solicitors: Solicitor[] }
  | { kind: 'not-loaded' }
  | { kind: 'invalid'; message?: string }
  | { kind: 'error' }

export type LocationResults = { location: string; state: LocationResult }[]

async function fetchLocation(location: string): Promise<LocationResult> {
  try {
    const res = await fetch(
      `${API_BASE}/conveyancing/solicitors?location=${encodeURIComponent(location)}`,
    )
    if (res.ok) {
      const solicitors = (await res.json()) as Solicitor[]
      return { kind: 'loaded', solicitors }
    }
    if (res.status === 503) return { kind: 'not-loaded' }
    if (res.status === 400) {
      const body = (await res.json().catch(() => null)) as { message?: string } | null
      return { kind: 'invalid', message: body?.message }
    }
    return { kind: 'error' }
  } catch {
    return { kind: 'error' }
  }
}

export default function HeroSection() {
  const [locations, setLocations] = useState<string[]>([])
  const [tags, setTags] = useState<string[]>([])
  const [results, setResults] = useState<LocationResults>([])
  const [searching, setSearching] = useState(false)

  useEffect(() => {
    fetch(`${API_BASE}/conveyancing/locations`)
      .then(r => r.json())
      .then((data: string[]) => setLocations(data))
      .catch(() => setLocations([]))
  }, [])

  async function handleSearch() {
    if (tags.length === 0) return
    setSearching(true)
    setResults(tags.map(location => ({ location, state: { kind: 'loading' } })))
    const settled = await Promise.allSettled(tags.map(fetchLocation))
    setResults(
      tags.map((location, i) => {
        const s = settled[i]
        return {
          location,
          state: s.status === 'fulfilled' ? s.value : { kind: 'error' },
        }
      }),
    )
    setSearching(false)
  }

  async function retry(location: string) {
    setResults(prev =>
      prev.map(r => (r.location === location ? { ...r, state: { kind: 'loading' } } : r)),
    )
    const state = await fetchLocation(location)
    setResults(prev => prev.map(r => (r.location === location ? { ...r, state } : r)))
  }

  return (
    <section id="center">
      <div className="hero">
        <div className="wordmark" role="img" aria-label="InfoTrack">
          <span className="wordmark-info">Info</span>
          <span className="wordmark-track">Track</span>
          <span className="wordmark-dot" aria-hidden="true" />
        </div>
      </div>
      <div>
        <h1>Find Solicitors</h1>
        <p>Select one or more locations to search for conveyancing solicitors.</p>
      </div>
      <LocationTagInput
        locations={locations}
        tags={tags}
        onAdd={loc => setTags(prev => [...prev, loc])}
        onRemove={loc => setTags(prev => prev.filter(t => t !== loc))}
      />
      <button
        type="button"
        className="counter"
        onClick={handleSearch}
        disabled={searching || tags.length === 0}
      >
        {searching ? 'Searching…' : 'Search Solicitors'}
      </button>
      <SearchResults results={results} onRetry={retry} />
    </section>
  )
}
