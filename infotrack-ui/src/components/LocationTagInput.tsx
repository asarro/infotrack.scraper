import { useState } from 'react'
import Tag from './Tag'

interface Props {
  locations: string[]
  tags: string[]
  onAdd: (loc: string) => void
  onRemove: (loc: string) => void
}

export default function LocationTagInput({ locations, tags, onAdd, onRemove }: Props) {
  const [input, setInput] = useState('')

  const available = locations.filter(l => !tags.includes(l))

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    const val = e.target.value
    if (locations.includes(val) && !tags.includes(val)) {
      onAdd(val)
      setInput('')
    } else {
      setInput(val)
    }
  }

  return (
    <div className="tag-input">
      {tags.map(loc => (
        <Tag key={loc} label={loc} onRemove={() => onRemove(loc)} />
      ))}
      <input
        className="tag-autocomplete"
        value={input}
        onChange={handleChange}
        placeholder={tags.length === 0 ? 'Type a location…' : ''}
        list="location-suggestions"
        disabled={available.length === 0}
      />
      <datalist id="location-suggestions">
        {available.map(loc => <option key={loc} value={loc} />)}
      </datalist>
    </div>
  )
}
