// The messages the server returned for one field ({ field: [messages] } from a 400 response), joined for display.
export const fieldMessages = (errors) => (field) => errors.value[field]?.join(' ') ?? '';
