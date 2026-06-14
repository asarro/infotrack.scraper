import { useState, useEffect } from 'react'
import heroImg from '../assets/hero.png'
import LocationTagInput from './LocationTagInput'
import './HeroSection.css'

const API_BASE = import.meta.env.VITE_API_URL ?? ''

type Status = 'idle' | 'loading' | 'success' | 'error'

export default function HeroSection() {
  const [locations, setLocations] = useState<string[]>([])
  const [tags, setTags] = useState<string[]>([])
  const [status, setStatus] = useState<Status>('idle')

  useEffect(() => {
    fetch(`${API_BASE}/conveyancing/locations`)
      .then(r => r.json())
      .then((data: string[]) => setLocations(data))
      .catch(() => setStatus('error'))
  }, [])

  async function handleSearch() {
    if (tags.length === 0) return
    setStatus('loading')
    try {
      const responses = await Promise.all(
        tags.map(loc =>
          fetch(`${API_BASE}/conveyancing/solicitors?location=${encodeURIComponent(loc)}`)
        )
      )
      setStatus(responses.every(r => r.ok) ? 'success' : 'error')
    } catch {
      setStatus('error')
    }
  }

  return (
    <section id="center">
      <div className="hero">
        <img src={heroImg} className="base" width="170" height="179" alt="" />
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
        disabled={status === 'loading' || tags.length === 0}
      >
        {status === 'loading' ? 'Searching…' : 'Search Solicitors'}
      </button>
      {status === 'success' && <p>Search initiated successfully.</p>}
      {status === 'error' && <p>Something went wrong. Please try again.</p>}
    </section>
  )
}
