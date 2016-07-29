import 'whatwg-fetch';
let fetch = window.fetch;

export default function requestJSON(inputData) {
	return fetch(inputData)
	// .then(response => {
	// 	console.log(response);
	// 	return response;
	// })
	.then(response => response.json());
	// Provide basic error handling, maybe retry logic?
}