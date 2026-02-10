export function getDisplayOrderId(publicId: string) {
  const normalized = publicId.replace(/[^a-zA-Z0-9]/g, "").toUpperCase();
  if (!normalized) {
    return "ORD-UNKNOWN";
  }
  return `ORD-${normalized.slice(0, 8)}`;
}
