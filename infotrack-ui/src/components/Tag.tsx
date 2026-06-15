interface Props {
  label: string
  onRemove: () => void
}

export default function Tag({ label, onRemove }: Props) {
  return (
    <span className="tag">
      {label}
      <button type="button" className="tag-remove" onClick={onRemove}>×</button>
    </span>
  )
}
