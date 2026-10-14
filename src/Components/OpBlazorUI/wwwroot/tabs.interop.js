export function scroll(element, amount) {
    if (!element) {
        return;
    }

    element.scrollBy({ left: amount, behavior: 'smooth' });
}
