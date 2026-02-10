interface InfoBannerProps {
  title: string;
  items: string[];
  tone?: "neutral" | "success";
}

export default function InfoBanner({
  title,
  items,
  tone = "neutral",
}: InfoBannerProps) {
  return (
    <section className={`info-banner ${tone === "success" ? "is-success" : ""}`}>
      <h3>{title}</h3>
      <ul>
        {items.map((item) => (
          <li key={item}>{item}</li>
        ))}
      </ul>
    </section>
  );
}
