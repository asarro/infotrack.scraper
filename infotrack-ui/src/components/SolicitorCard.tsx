import type { Solicitor } from './HeroSection'

function normalizeUrl(url: string) {
  return /^https?:\/\//i.test(url) ? url : `https://${url}`
}

function displayUrl(url: string) {
  return url.replace(/^https?:\/\//i, '').replace(/\/$/, '')
}

export default function SolicitorCard({ solicitor }: { solicitor: Solicitor }) {
  const { name, address, phone, description, website } = solicitor

  return (
    <article className="solicitor-card">
      <span className="solicitor-icon" aria-hidden="true">
        ⚖
      </span>
      <div className="solicitor-body">
        <h3 className="solicitor-name">{name}</h3>
        {address && <p className="solicitor-line">{address}</p>}
        {phone && (
          <p className="solicitor-line">
            <a href={`tel:${phone}`}>{phone}</a>
          </p>
        )}
        {description && <p className="solicitor-desc">{description}</p>}
        {website && (
          <a
            className="solicitor-website"
            href={normalizeUrl(website)}
            target="_blank"
            rel="noreferrer"
          >
            {displayUrl(website)} <span aria-hidden="true">→</span>
          </a>
        )}
      </div>
    </article>
  )
}
