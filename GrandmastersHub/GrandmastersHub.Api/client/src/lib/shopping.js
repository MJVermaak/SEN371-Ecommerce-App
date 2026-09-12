const currency = new Intl.NumberFormat('en-ZA', {
  style: 'currency', currency: 'ZAR', minimumFractionDigits: 2, maximumFractionDigits: 2,
});
export const money = (value) => currency.format(Number(value));

export function collectionPath(name = '') {
  const value = name.toLowerCase();
  const collection = ['boards', 'clocks', 'books', 'bespoke'].find((key) => value.includes(key));
  return collection ? `/${collection}` : '/boards';
}

export function fallbackImage(category = '') {
  return {
    '/boards': '/images/Shopping-plain-chess-board.jpg',
    '/clocks': '/images/Shopping-clock-1.png',
    '/books': '/images/book-1.png',
    '/bespoke': '/images/Volcanic.png',
  }[collectionPath(category)];
}

export function safeReturnTo(value) {
  // Only application routes can be post-login destinations.
  return typeof value === 'string' && /^\/(product\/[1-9]\d*|orders\/[1-9]\d*|cart|checkout|orders|boards|books|clocks|bespoke|profile)$/.test(value)
    ? value : '/profile';
}

export function checkoutKey() {
  if (globalThis.crypto?.randomUUID) return globalThis.crypto.randomUUID();
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (character) => {
    const random = Math.floor(Math.random() * 16);
    return (character === 'x' ? random : (random & 3) | 8).toString(16);
  });
}

export function remainingQuantity(stock, inCart = 0) {
  return Math.max(0, Math.min(99, Number(stock) || 0) - Math.max(0, Number(inCart) || 0));
}

export function validQuantity(value, maximum) {
  return Number.isInteger(value) && value >= 1 && value <= maximum;
}
