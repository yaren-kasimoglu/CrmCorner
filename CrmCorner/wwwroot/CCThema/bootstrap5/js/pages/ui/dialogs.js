document.querySelector(".sa-basic").addEventListener('click', function(){
  Swal.fire("Our First Alert");
});
document.querySelector(".sa-title-text").addEventListener('click', function(){
  Swal.fire(
      'The Internet?',
      'That thing is still around?',
      'question'
  )
});
document.querySelector(".sa-title-error").addEventListener('click', function(){
  Swal.fire({
      icon: 'error',
      title: 'Oops...',
      text: 'Something went wrong!',
      footer: '<a href="">Why do I have this issue?</a>'
  })
});
document.querySelector(".sa-buttons").addEventListener('click', function(){
  Swal.fire({
  title: 'Do you want to save the changes?',
  showDenyButton: true,
  showCancelButton: true,
  confirmButtonText: 'Save',
  denyButtonText: `Don't save`,
  }).then((result) => {
  /* Read more about isConfirmed, isDenied below */
  if (result.isConfirmed) {
      Swal.fire('Saved!', '', 'success')
      } else if (result.isDenied) {
          Swal.fire('Changes are not saved', '', 'info')
      }
  })
});
document.querySelector(".sa-position").addEventListener('click', function(){
  Swal.fire({
      position: 'top-end',
      icon: 'success',
      title: 'Your work has been saved',
      showConfirmButton: false,
      timer: 1500
  })
});
document.querySelector(".sa-image").addEventListener('click', function(){
  Swal.fire({
      title: 'Sweet!',
      text: 'Modal with a custom image.',
      imageUrl: 'assets/images/image-gallery/1.jpg',
      imageWidth: 400,
      imageHeight: 200,
      imageAlt: 'Custom image',
  })
});
document.querySelector(".sa-autoclose").addEventListener('click', function(){
  let timerInterval
  Swal.fire({
      title: 'Auto close alert!',
      html: 'I will close in <b></b> milliseconds.',
      timer: 2000,
      timerProgressBar: true,
      didOpen: () => {
          Swal.showLoading()
          const b = Swal.getHtmlContainer().querySelector('b')
          timerInterval = setInterval(() => {
          b.textContent = Swal.getTimerLeft()
          }, 100)
      },
      willClose: () => {
          clearInterval(timerInterval)
      }
      }).then((result) => {
      /* Read more about handling dismissals below */
      if (result.dismiss === Swal.DismissReason.timer) {
          console.log('I was closed by the timer')
      }
  })
});
document.querySelector(".sa-ajax").addEventListener('click', function(){
  Swal.fire({
      title: 'Submit your Github username',
      input: 'text',
      inputAttributes: {
          autocapitalize: 'off'
      },
      showCancelButton: true,
      confirmButtonText: 'Look up',
      showLoaderOnConfirm: true,
      preConfirm: (login) => {
          return fetch(`//api.github.com/users/${login}`)
          .then(response => {
              if (!response.ok) {
              throw new Error(response.statusText)
              }
              return response.json()
          })
          .catch(error => {
              Swal.showValidationMessage(
              `Request failed: ${error}`
              )
          })
      },
      allowOutsideClick: () => !Swal.isLoading()
      }).then((result) => {
      if (result.isConfirmed) {
          Swal.fire({
          title: `${result.value.login}'s avatar`,
          imageUrl: result.value.avatar_url
          })
      }
  })
});