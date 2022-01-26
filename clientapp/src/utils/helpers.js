function nullReject(propertyName) {
  return Promise.reject(`${propertyName} is empty`);
}

function debounce(fn, delay) {
  let timeout = null;

  return function (...args) {
    clearTimeout(timeout);
    const that = this;

    timeout = setTimeout(() => fn.apply(that, args), delay);
  };
}

function isEmailValid(email) {
  return /\S+@\S+\.\S+/.test(email);
}

export { nullReject, debounce, isEmailValid };
